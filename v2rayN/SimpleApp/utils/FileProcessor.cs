using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApp.utils;
public class FileProcessor
{
    private readonly string _downloadedFilePath;
    private readonly string _tempDirectory;
    public FileProcessor(string downloadedFilePath)
    {
        // 如果 downloadedFilePath 仅为文件名，补全为当前运行目录的完整路径
        if (!Path.IsPathRooted(downloadedFilePath))
        {
            _downloadedFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, downloadedFilePath);
        }
        else
        {
            _downloadedFilePath = downloadedFilePath;
        }

        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    public async Task ProcessAsync()
    {
        try
        {
            // 解压文件到临时目录
            await ExtractToTempDirectoryAsync();

            // 遍历解压后的文件并处理
            await ProcessExtractedFilesAsync();

            // 清理临时目录
            Cleanup();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"处理文件失败: {ex.Message}");
            throw;
        }
    }

    private async Task ExtractToTempDirectoryAsync()
    {
        try
        {
            if (Path.GetExtension(_downloadedFilePath).ToLower() == ".zip")
            {
                using (var archive = ZipFile.OpenRead(_downloadedFilePath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        string destinationPath = Path.Combine(_tempDirectory, entry.FullName);
                        string destinationDir = Path.GetDirectoryName(destinationPath);

                        if (!Directory.Exists(destinationDir))
                        {
                            Directory.CreateDirectory(destinationDir);
                        }

                        if (!string.IsNullOrEmpty(entry.Name)) // 排除目录
                        {
                            entry.ExtractToFile(destinationPath, true);
                        }
                    }
                }
            }
            else
            {
                // 如果不是zip文件，直接复制到临时目录
                string fileName = Path.GetFileName(_downloadedFilePath);
                string destinationPath = Path.Combine(_tempDirectory, fileName);
                File.Copy(_downloadedFilePath, destinationPath, true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"解压文件失败: {ex.Message}");
            throw;
        }
    }

    private async Task ProcessExtractedFilesAsync()
    {
        try
        {
            string[] extractedFiles = Directory.GetFiles(_tempDirectory, "*", SearchOption.AllDirectories);

            // 获取目标目录
            string targetBaseDir;
            if (string.IsNullOrEmpty(Path.GetDirectoryName(_downloadedFilePath)))
            {
                // 如果 _downloadedFilePath 仅为文件名，使用当前运行目录
                targetBaseDir = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                // 否则使用文件的目录
                targetBaseDir = Path.GetDirectoryName(_downloadedFilePath);
            }

            // 删除目标目录下所有.bak文件
            string[] bakFiles = Directory.GetFiles(targetBaseDir, "*.bak", SearchOption.AllDirectories);
            foreach (string bakFile in bakFiles)
            {
                File.Delete(bakFile);
                Console.WriteLine($"已删除备份文件: {bakFile}");
            }

            // 获取解压后的顶层文件夹（假设 ZIP 包有一个顶层目录）
            string topLevelDir = GetTopLevelDirectory(_tempDirectory);

            foreach (string extractedFile in extractedFiles)
            {
                // 计算相对路径，去掉顶层文件夹
                string relativePath = extractedFile.Substring(_tempDirectory.Length + 1);
                if (!string.IsNullOrEmpty(topLevelDir) && relativePath.StartsWith(topLevelDir + Path.DirectorySeparatorChar))
                {
                    // 移除顶层文件夹部分
                    relativePath = relativePath.Substring(topLevelDir.Length + 1);
                }

                // 如果 relativePath 为空（例如只有顶层目录），跳过
                if (string.IsNullOrEmpty(relativePath))
                {
                    continue;
                }

                string targetPath = Path.Combine(targetBaseDir, relativePath);
                string targetDir = Path.GetDirectoryName(targetPath);

                // 确保目标目录存在
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                // 如果目标文件存在，备份为.bak
                if (File.Exists(targetPath))
                {
                    string backupPath = targetPath + ".bak";
                    File.Move(targetPath, backupPath);
                    Console.WriteLine($"已备份旧文件: {targetPath} -> {backupPath}");
                }

                // 复制新文件到目标位置
                await Task.Run(() => File.Copy(extractedFile, targetPath, true));
                Console.WriteLine($"已复制新文件: {extractedFile} -> {targetPath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"处理提取文件失败: {ex.Message}");
            throw;
        }
    }

    private string GetTopLevelDirectory(string tempDir)
    {
        try
        {
            // 获取临时目录下的所有子目录
            var subDirs = Directory.GetDirectories(tempDir, "*", SearchOption.TopDirectoryOnly);
            if (subDirs.Length == 1)
            {
                // 如果只有一个子目录，假设它是顶层目录
                return Path.GetFileName(subDirs[0]);
            }
            // 如果没有子目录或有多个子目录，返回空字符串
            return string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取顶层目录失败: {ex.Message}");
            return string.Empty;
        }
    }

    private void Cleanup()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
                Console.WriteLine($"已清理临时目录: {_tempDirectory}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"清理临时目录失败: {ex.Message}");
        }
    }

    public void Dispose()
    {
        Cleanup();
    }
}
