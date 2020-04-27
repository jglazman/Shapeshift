//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Glazman.Shapeshift
{
	/// <summary>
	/// This was a fun warmup excercise on a Sunday morning... always looking for a better logging solution in Unity!
	/// The idea is to unify log formatting and error reporting throughout the project.  In a real production environment
	/// I would also subscribe to Unity's LogHandler and write everything to a file that could easily be shared by QA,
	/// attached to bug tickets, parsed by tools, etc.
	/// </summary>
	public static class Logger
	{
		private static MethodBase UnityLogErrorMethod;
		
		static Logger()
		{
			// this hack found on Unity Answers forum: http://answers.unity.com/answers/788744/view.html
			UnityLogErrorMethod = typeof(UnityEngine.Debug).GetMethod("LogPlayerBuildError", BindingFlags.NonPublic | BindingFlags.Static);
		}

		[Conditional("UNITY_EDITOR")]
		public static void LogEditor(string message, UnityEngine.Object context=null)
		{
			// this causes the console to bring you to the wrong line of code ;(
			// the LogPlayerBuildError hack only works for errors.
			// but this gives better output than Unity's default logger, so it's still an improvement.
			Debug.Log(FormatMessage(message, false), context);
		}

		[Conditional("UNITY_EDITOR")]
		public static void LogWarningEditor(string message, UnityEngine.Object context=null) 
		{
			Debug.LogWarning(FormatMessage(message, false), context);
		}

		// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/caller-information
		public static void LogError(string message, 
			[CallerFilePath] string sourceFilePath="",
			[CallerLineNumber] int sourceLineNumber=0)
		{
			if (UnityLogErrorMethod != null)
				UnityLogErrorMethod.Invoke(null, new object[] { FormatMessage(message), FormatFilename(sourceFilePath), sourceLineNumber, 0 });
			else
				Debug.LogError(FormatMessage(message, false));
		}

		private static string FormatFilename(string pathToFile)
		{
			// format the path so Unity will jump to the correct line when we double-click the Console
			return pathToFile.Replace(Application.dataPath, "Assets");
		}

		private static string FormatStackFrame(StackFrame frame)
		{
			string s = frame.ToString();
			int n = s.IndexOf("Assets", StringComparison.Ordinal);	// strip the leading garbage
			return $"{frame.GetMethod().DeclaringType}.{frame.GetMethod().Name} ({(n > 0 ? s.Substring(n) : s)})";
		}
		
		private static string FormatStackTrace(IEnumerable<StackFrame> stackFrames)
		{
			// this logger always adds 2 lines to the stacktrace that we don't care about
			var formattedFrames = stackFrames.Skip(2).Select(FormatStackFrame);
			return string.Join("\n", formattedFrames);
		}

		private static string FormatMessage(string message, bool printFullStackTrace=true)
		{
			var stackTrace = new StackTrace(true);
			var stackFrames = stackTrace.GetFrames();
			if (stackFrames == null)
				return $"[{Time.time}] {message}";
			
			var callerFrame = stackFrames.FirstOrDefault(f => f.GetMethod().DeclaringType != typeof(Logger)) ?? stackFrames.First();
			//var callerType = callerFrame.GetMethod()?.DeclaringType;	// this gives mangled names when called from a coroutine
			var callerType = System.IO.Path.GetFileNameWithoutExtension(callerFrame.GetFileName());	// this looks better

			if (printFullStackTrace)
				return $"[{Time.time}] [{callerType}] {message}\n{FormatStackTrace(stackFrames)}";
			
			return $"[{Time.time}] [{callerType}] {message}\n{FormatStackFrame(callerFrame)}";
		}
	}
}
