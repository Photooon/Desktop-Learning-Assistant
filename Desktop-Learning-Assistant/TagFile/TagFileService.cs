﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopLearningAssistant.TagFile.Model;
using DesktopLearningAssistant.TagFile.Expression;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace DesktopLearningAssistant.TagFile
{
    public class TagFileService : ITagFileService
    {
        #region Tag 相关操作

        /// <summary>
        /// 按 TagName 获取 Tag
        /// </summary>
        /// <returns>不存在则返回 null</returns>
        public async Task<Tag> GetTagAsync(string tagName)
        {
            using (var context = Context)
            {
                return await context.Tags.FindAsync(tagName);
            }
        }

        /// <summary>
        /// 将新 Tag 加入系统中，并返回添加的 Tag 对象。
        /// 若该名字的 Tag 已存在，则什么都不做。
        /// </summary>
        public async Task<Tag> AddTagAsync(string tagName)
        {
            using (var context = Context)
            {
                Tag tag = await GetTagAsync(tagName);
                if (tag == null)
                {
                    tag = new Tag { TagName = tagName };
                    await context.Tags.AddAsync(tag);
                    await context.SaveChangesAsync();
                }
                return tag;
            }
        }

        /// <summary>
        /// 将该 Tag 从系统中移除。
        /// 若 Tag 不存在，则什么都不做。
        /// </summary>
        public async Task RemoveTagAsync(Tag tag)
        {
            using (var context = Context)
            {
                if (await IsTagExistAsync(tag.TagName))
                {
                    context.Tags.Remove(tag);
                    await context.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// 重命名该 Tag。
        /// 若新名字的 Tag 已经存在，则什么都不做并返回 false。
        /// </summary>
        public async Task<bool> RenameTagAsync(Tag tag, string newName)
        {
            using (var context = Context)
            {
                if (!await IsTagExistAsync(newName))
                {
                    tag.TagName = newName;
                    await context.SaveChangesAsync();
                    return true;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// 该名字的 Tag 是否存在
        /// </summary>
        public async Task<bool> IsTagExistAsync(string tagName)
        {
            using (var context = Context)
            {
                return await GetTagAsync(tagName) != null;
            }
        }

        /// <summary>
        /// 获取含所有 Tag 的 List
        /// </summary>
        public async Task<List<Tag>> TagListAsync()
        {
            using (var context = Context)
            {
                return await context.Tags.ToListAsync();
            }
        }

        #endregion

        #region TagFileRelation 有关操作

        /// <summary>
        /// 获取 TagFileRelation 对象。
        /// </summary>
        /// <returns>不存在则返回 null</returns>
        public async Task<TagFileRelation> GetRelationAsync(Tag tag, FileItem fileItem)
        {
            using (var context = Context)
            {
                return await context.Relations.FindAsync(tag.TagName, fileItem.FileItemId);
            }
        }

        /// <summary>
        /// 插入一个新的 Tag-FileItem 关系，并返回实体类对象。
        /// 若关系已存在，则什么都不做。
        /// </summary>
        /// <returns>关系实体类</returns>
        public async Task<TagFileRelation> AddRelationAsync(Tag tag, FileItem fileItem)
        {
            using (var context = Context)
            {
                var relation = await GetRelationAsync(tag, fileItem);
                if (relation == null)
                {
                    relation = new TagFileRelation
                    {
                        TagName = tag.TagName,
                        FileItemId = fileItem.FileItemId,
                        UtcCreateTime = DateTime.UtcNow
                    };
                    await context.Relations.AddAsync(relation);
                    await context.SaveChangesAsync();
                }
                return relation;
            }
        }

        /// <summary>
        /// 移除 TagFileRelation 对象。
        /// 若关系不存在则什么都不做。
        /// </summary>
        public async Task RemoveRelationAsync(TagFileRelation relation)
        {
            using (var context = Context)
            {
                if (await IsRelationExistAsync(relation.Tag, relation.FileItem))
                {
                    context.Relations.Remove(relation);
                    await context.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// 移除某个 Tag-FileItem 关系。
        /// 若该关系不存在，则什么都不做。
        /// </summary>
        public async Task RemoveRelationAsync(Tag tag, FileItem fileItem)
        {
            using (var context = Context)
            {
                var relation = await GetRelationAsync(tag, fileItem);
                if (relation != null)
                {
                    context.Relations.Remove(relation);
                    await context.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// 查询关系是否存在
        /// </summary>
        public async Task<bool> IsRelationExistAsync(Tag tag, FileItem fileItem)
        {
            return (await GetRelationAsync(tag, fileItem)) != null;
        }

        /// <summary>
        /// 获取含所有关系的列表
        /// </summary>
        public async Task<List<TagFileRelation>> RelationListAsync()
        {
            using (var context = Context)
            {
                return await context.Relations.ToListAsync();
            }
        }

        #endregion

        #region FileItem 有关操作

        /// <summary>
        /// 按 FileItemId 获取 FileItem
        /// </summary>
        public async Task<FileItem> GetFileItemAsync(int fileItemId)
        {
            using (var context = Context)
            {
                return await context.FileItems.FindAsync(fileItemId);
            }
        }

        /// <summary>
        /// 将文件以快捷方式的形式加入仓库
        /// </summary>
        /// <param name="filepath">文件路径</param>
        public async Task<FileItem> AddShortcutToRepoAsync(string filepath)
        {
            string targetName = Path.GetFileName(filepath);
            string displayName = targetName + ".lnk";
            string shortcutName = FileUtils.GetAvailableFileName(displayName, RepoPath);
            FileUtils.CreateShortcut(filepath, FileUtils.FileInFolder(RepoPath, shortcutName));
            var fileItem = new FileItem
            {
                DisplayName = displayName,
                RealName = shortcutName
            };
            await AddFileItemAsync(fileItem);
            return fileItem;
        }

        /// <summary>
        /// 将文件移动到仓库中
        /// </summary>
        public async Task<FileItem> MoveFileToRepoAsync(string filepath)
        {
            string realName = await Task.Run(
                () => FileUtils.MoveFileAutoNumber(filepath, RepoPath));
            var fileItem = new FileItem
            {
                DisplayName = Path.GetFileName(filepath),
                RealName = realName
            };
            await AddFileItemAsync(fileItem);
            return fileItem;
        }

        /// <summary>
        /// 获取文件在仓库内的真实路径
        /// </summary>
        public string GetRealFilepath(FileItem fileItem)
            => FileUtils.FileInFolder(RepoPath, fileItem.RealName);

        /// <summary>
        /// 删除文件
        /// </summary>
        public async Task DeleteFileAsync(FileItem fileItem)
        {
            using (var context = Context)
            {
                string path = GetRealFilepath(fileItem);
                if (File.Exists(path))
                {
                    await Task.Run(() => File.Delete(path));
                }
                context.FileItems.Remove(fileItem);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 将文件移动到回收站
        /// </summary>
        public async Task DeleteFileToRecycleBinAsync(FileItem fileItem)
        {
            using (var context = Context)
            {
                if (File.Exists(GetRealFilepath(fileItem)))
                {
                    //move to temp recycle first
                    string curFilename = FileUtils.MoveFileAutoNumber(
                        GetRealFilepath(fileItem), TempRecyclePath);
                    string curPath = Path.Combine(TempRecyclePath, curFilename);
                    //then send to system recycle bin
                    await Task.Run(() => FileUtils.DeleteFileToRecycleBin(curPath));
                }
                context.FileItems.Remove(fileItem);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 重命名文件
        /// </summary>
        public async Task RenameFileItemAsync(FileItem fileItem, string newName)
        {
            using (var context = Context)
            {
                fileItem.DisplayName = newName;
                fileItem.RealName = await Task.Run(() =>
                    FileUtils.RenameFileAutoNumber(GetRealFilepath(fileItem), newName));
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 获取含所有 FileItem 的列表
        /// </summary>
        public async Task<List<FileItem>> FileItemListAsync()
        {
            using (var context = Context)
            {
                return await context.FileItems.ToListAsync();
            }
        }

        /// <summary>
        /// 添加 FileItem
        /// </summary>
        private async Task AddFileItemAsync(FileItem fileItem)
        {
            using (var context = Context)
            {
                await context.FileItems.AddAsync(fileItem);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// only for test adding file item
        /// </summary>
        public async Task<FileItem> AddFileItemForTestAsync(string dispName, string realName)
        {
            using (var context = Context)
            {
                var file = new FileItem { DisplayName = dispName, RealName = realName };
                await context.FileItems.AddAsync(file);
                await context.SaveChangesAsync();
                return file;
            }
        }

        #endregion

        #region 查询有关操作

        /// <summary>
        /// 表达式查询
        /// </summary>
        /// <exception cref="InvalidExpressionException">查询表达式非法</exception>
        public async Task<List<FileItem>> QueryAsync(string expression)
        {
            using (var context = Context)
            {
                var files = new List<FileItem>();
                var idList = TagExpression.Query(context.Relations, expression);
                foreach (int fileItemId in idList)
                    files.Add(await GetFileItemAsync(fileItemId));
                return files;
            }
        }

        /// <summary>
        /// 获取不含任何标签的文件
        /// </summary>
        public async Task<List<FileItem>> FilesWithoutTagAsync()
        {
            using (var context = Context)
            {
                var hasTagIds = new HashSet<int>();
                (await RelationListAsync()).ForEach(
                    relation => hasTagIds.Add(relation.FileItemId));
                var allFiles = await FileItemListAsync();
                var files = new List<FileItem>();
                foreach (var file in allFiles)
                    if (!hasTagIds.Contains(file.FileItemId))
                        files.Add(file);
                return files;
            }
        }

        #endregion

        #region 静态方法

        /// <summary>
        /// 获取单例对象
        /// </summary>
        public static ITagFileService GetService()
        {
            if (uniqueService == null)
            {
                lock (locker)
                {
                    if (uniqueService == null)
                        uniqueService = new TagFileService();
                }
            }
            return uniqueService;
        }

        /// <summary>
        /// 确保数据库和仓库文件夹已创建
        /// </summary>
        public static void EnsureDbAndFolderCreated()
        {
            EnsureDbAndFolderCreatedAsync().Wait();
        }

        /// <summary>
        /// 确保数据库和仓库文件夹已创建（异步版本）
        /// </summary>
        public static async Task EnsureDbAndFolderCreatedAsync()
        {
            var builder = new DbContextOptionsBuilder<TagFileContext>();
            builder.UseSqlite($"Data Source={TagFileConfig.DbPath}");
            using (var context = new TagFileContext(builder.Options))
            {
                await context.Database.EnsureCreatedAsync();
            }
            //ensure folders created
            Directory.CreateDirectory(TagFileConfig.RepoPath);
            Directory.CreateDirectory(TagFileConfig.TempRecyclePath);
        }

        #endregion

        protected TagFileService()
        {
            /*
            var builder = new DbContextOptionsBuilder<TagFileContext>();
            builder.UseSqlite($"Data Source={TagFileConfig.DbPath}");
            context = new TagFileContext(builder.Options);
            */
        }

        /// <summary>
        /// 文件仓库的路径
        /// </summary>
        private string RepoPath { get => TagFileConfig.RepoPath; }

        /// <summary>
        /// 临时回收站的路径
        /// </summary>
        private string TempRecyclePath { get => TagFileConfig.TempRecyclePath; }

        //private readonly TagFileContext context;
        /// <summary>
        /// 用于操作数据库的 DbContext
        /// </summary>
        private TagFileContext Context
        {
            get
            {
                var builder = new DbContextOptionsBuilder<TagFileContext>();
                builder.UseSqlite($"Data Source={TagFileConfig.DbPath}");
                TagFileContext context = new TagFileContext(builder.Options);
                return context;
            }
        }

        /// <summary>
        /// 单例对象
        /// </summary>
        private static volatile TagFileService uniqueService = null;

        /// <summary>
        /// 互斥锁，用于保证单例模式的实现是线程安全的
        /// </summary>
        private static readonly object locker = new object();
    }

    //TODO modify this to a config class
    class TagFileConfig
    {
        public static string RepoPath { get; } = "C:/Users/zhb/Desktop/temp/tag-file/repo";
        public static string DbPath { get; } = "C:/Users/zhb/Documents/sqlitedb/TagFileDB.db";
        public static string TempRecyclePath { get; } = "C:/Users/zhb/Desktop/temp/tag-file/temp-recycle";
    }
}
