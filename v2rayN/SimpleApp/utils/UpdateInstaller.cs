using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApp.utils;
public static class UpdateInstaller
{
    /// <summary>
    /// 将 <paramref name="zipPath"/> 解压到固定目录
    ///   [installDir]\\UpdateFile\\UnzipFile
    /// 然后把内容复制到 <paramref name="installDir"/>，
    /// 替换前把原文件改名为 .bak。
    /// </summary>
    public static void Apply(string zipPath, string installDir)
    {
        var updateRoot = Path.Combine(installDir, "UpdateFile");
        var unzipDir = Path.Combine(updateRoot, "UnzipFile");

        // ① 确保 UpdateFile 目录存在；如有上次残留的 UnzipFile 先删掉
        if (Directory.Exists(unzipDir))
            Directory.Delete(unzipDir, true);
        Directory.CreateDirectory(unzipDir);

        // ② 解压到 UnzipFile
        ZipFile.ExtractToDirectory(zipPath, unzipDir);

        // ③ 备份原文件 → 覆盖
        foreach (var src in Directory.GetFiles(unzipDir, "*",
                         SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(unzipDir, src);
            var dst = Path.Combine(installDir, rel);

            Directory.CreateDirectory(Path.GetDirectoryName(dst)!);

            if (File.Exists(dst))
            {
                var bak = dst + ".bak";
                if (File.Exists(bak))
                    File.Delete(bak);  // 清理旧备份
                MoveFileEx(dst, bak,
                    MoveFileFlags.ReplaceExisting | MoveFileFlags.WriteThrough);
            }

            MoveFileEx(src, dst,
                MoveFileFlags.ReplaceExisting | MoveFileFlags.WriteThrough);
        }

        // ④ 可保留 UnzipFile 供调试；若想节省空间可删除：
        // Directory.Delete(unzipDir, true);
    }

    // ---------------- Win32 原子覆盖 ----------------
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool MoveFileEx(string oldName,
                                          string? newName,
                                          MoveFileFlags flags);

    [Flags]
    private enum MoveFileFlags : uint
    {
        ReplaceExisting = 0x1,
        WriteThrough = 0x8
    }
}
