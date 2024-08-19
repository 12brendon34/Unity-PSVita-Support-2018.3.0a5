using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace UnityEditor.PSP2
{
	internal class ExternalTool
	{
		internal static StreamReader StartProcess(string processFileName, string inputArguments, string outputFile, bool waitForExit, string workingDir)
		{
			Console.WriteLine("### StartProcess: " + processFileName + " " + inputArguments);
			Process process = new Process();
			process.StartInfo.FileName = processFileName;
			process.StartInfo.Arguments = inputArguments;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardOutput = true;
			if (workingDir.Trim() != "")
			{
				process.StartInfo.WorkingDirectory = workingDir;
			}
			process.Start();
			if (waitForExit)
			{
				string s = process.StandardOutput.ReadToEnd();
				byte[] bytes = Encoding.ASCII.GetBytes(s);
				Stream stream = new MemoryStream(bytes);
				StreamReader result = new StreamReader(stream);
				process.WaitForExit();
				if (process.ExitCode != 0 || (outputFile.Trim() != "" && !File.Exists(outputFile)))
				{
					UnityEngine.Debug.Log(processFileName + " " + inputArguments);
					throw new Exception(processFileName + " -> " + outputFile + " : " + process.StandardError.ReadToEnd());
				}
				return result;
			}
			return process.StandardOutput;
		}
	}
}
