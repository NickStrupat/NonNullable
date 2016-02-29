using System;
using System.IO;
using System.Reflection;

using Mono.Cecil;

using NUnit.Framework;

[TestFixture]
public class WeaverTests {
	private Assembly assembly;
	private String newAssemblyPath;
	private String assemblyPath;

	[TestFixtureSetUp]
	public void Setup() {
		var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\AssemblyToProcess\AssemblyToProcess.csproj"));
		this.assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin\Debug\AssemblyToProcess.dll");
#if (!DEBUG)
        assemblyPath = assemblyPath.Replace("Debug", "Release");
#endif

		this.newAssemblyPath = this.assemblyPath/*.Replace(".dll", "2.dll");
		File.Copy(this.assemblyPath, this.newAssemblyPath, true)*/;

		var moduleDefinition = ModuleDefinition.ReadModule(this.newAssemblyPath);
		try {
			moduleDefinition.Assembly.MainModule.ReadSymbols();
		}
		catch (InvalidOperationException ex) {
			throw new Exception("Make sure Mono.Cecil.Pdb and/or Mono.Cecil.Mdb is/are referenced.", ex);
		}
		var weavingTask = new ModuleWeaver {
			ModuleDefinition = moduleDefinition
		};

		weavingTask.Execute();
		moduleDefinition.Write(this.newAssemblyPath);

		this.assembly = Assembly.LoadFile(this.newAssemblyPath);
	}

#if (DEBUG)
	[Test]
	public void PeVerify() {
		Verifier.Verify(this.assemblyPath, this.newAssemblyPath);
	}
#endif

	[Test]
	public void ValidateBasicScenario() {
		var t = this.assembly.GetType("AssemblyToProcess.ClassToProcess", throwOnError: true);
		var m = t.GetMethod("What");
		m.Invoke(null, null);


		//Assert.AreEqual("Hello World", instance.World());
	}
}