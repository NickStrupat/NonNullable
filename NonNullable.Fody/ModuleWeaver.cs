using System;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Cecil.Cil;
using NonNullable;

public class ModuleWeaver {
	// Will log an informational message to MSBuild
	public Action<string> LogInfo { get; set; }

	// An instance of Mono.Cecil.ModuleDefinition for processing
	public ModuleDefinition ModuleDefinition { get; set; }

	TypeSystem typeSystem;

	// Init logging delegates to make testing easier
	public ModuleWeaver() {
		LogInfo = m => { };
	}

	public void Execute() {
		this.nntr = this.ModuleDefinition.ImportReference(typeof(NonNullable<>));
		var allMethods = ModuleDefinition.GetTypes().SelectMany(x => x.Methods.Where(y => y.HasBody)).ToArray();
		foreach (var method in allMethods)
			ProcessMethod(method);
	}

	private TypeReference nntr;

	private void ProcessMethod(MethodDefinition method) {
		// Find obvious mistakes
		// - Uninitialized NonNullable<T> locals, instance members, static members
		if (method.IsConstructor) {
			if (method.IsStatic) {
				var staticMembers = method.DeclaringType.Fields.Where(x => x.IsStatic && this.IsNonNullable(x)).ToArray();
				//var referenced = method.Body.Instructions.Any(x => (x.OpCode == OpCodes.Stsfld || x.OpCode == OpCodes.Ldsfld || x.OpCode == OpCodes.Ldsflda) && ((MethodReference)x.Operand).);
			}
			else {
				var members = method.DeclaringType.Fields.Where(x => !x.IsStatic && this.IsNonNullable(x)).ToArray();
			}
		}
		var vars = method.Body.Variables.Where(this.IsNonNullable).Select(x => new VarMapping(x)).ToArray();
		foreach (var instruction in method.Body.Instructions) {
			if (instruction.OpCode == OpCodes.Ldloca || instruction.OpCode == OpCodes.Ldloca_S) {
				var var = vars.Single(x => x.Var == instruction.Operand);
				var.Instruction = instruction;
				switch (var.Initialized) {
					case false:
						throw GetNotSupportedException(var);
					case true:
						continue;
					case null:
						var call = instruction.Next?.Next;
						if (call?.OpCode == OpCodes.Call) {
							var mr = (MethodReference) call.Operand;
							var t = ((GenericInstanceType) mr.DeclaringType).GenericArguments.SingleOrDefault();
							var nngit = this.nntr.MakeGenericInstanceType(t);
							var isCallingTheNonNullableMainConstructor = mr.DeclaringType.FullName == nngit.FullName && mr.Parameters.Count == 1;
							var isPassingNull = call.Previous.OpCode == OpCodes.Ldnull;
							var.Initialized = isCallingTheNonNullableMainConstructor && !isPassingNull;
						}
						//else if (call?.OpCode == OpCodes.Newobj) {
							
						//}
						else {
							var.Initialized = false;
							var.Instruction = method.Body.Instructions.First();
						}
						break;
				}
			}
		}
		for (var i = 0; i < vars.Length; i++) {
			var var = vars[i];
			if (var.Initialized != true) {
				var.Instruction = var.Instruction ?? method.Body.Instructions.First();
				throw GetNotSupportedException(var);
			}
		}

		// 


		////var asdf = new GenericInstanceType(new TypeReference("NonNullable", "NonNullable", ModuleDefinition.));
		//foreach (var instruction in method.Body.Instructions) {
		//	if (instruction.OpCode == OpCodes.Initobj) {
		//		var git = instruction.Operand as GenericInstanceType;
		//		var ga1 = git.GenericArguments.SingleOrDefault();
		//		if (ga1 != null) {
		//			var trgit = nntr.MakeGenericInstanceType(ga1);
		//			if (trgit.FullName == git.FullName)
		//				throw GetNotSupportedException(instruction);
		//		}
		//	}
		//}
	}

	private Boolean IsNonNullable(VariableDefinition variableDefinition) {
		var git = variableDefinition.VariableType as GenericInstanceType;
		if (git == null || git.GenericArguments.Count != 1)
			return false;
		var nngit = this.nntr.MakeGenericInstanceType(git.GenericArguments.Single());
		return git.FullName == nngit.FullName;
	}

	private Boolean IsNonNullable(FieldDefinition variableDefinition) {
		//return variableDefinition.Resolve().FullName == nntr.FullName;
		var git = variableDefinition.FieldType as GenericInstanceType;
		if (git == null || git.GenericArguments.Count != 1)
			return false;
		var nngit = this.nntr.MakeGenericInstanceType(git.GenericArguments.Single());
		return git.FullName == nngit.FullName;
	}

	private static NotSupportedException GetNotSupportedException(VarMapping varMapping) {
		String exceptionMessage = $"Use of unnassigned non-nullable local variable '{varMapping.Var.Name}'.{Environment.NewLine}{GetSequencePointText(varMapping.Instruction)}";
		return new NotSupportedException(exceptionMessage);
	}

	private static String GetSequencePointText(Instruction i) {
		while (i.SequencePoint == null && i.Previous != null) // Look for last sequence point
			i = i.Previous;
		if (i.SequencePoint == null)
			return "No source line information available. Symbols required for source line.";
		return $"Source: {i.SequencePoint.Document.Url}:line {i.SequencePoint.StartLine}";
	}
}

[DebuggerDisplay("{Var} : {Initialized}")]
internal class VarMapping {
	public VarMapping(VariableDefinition var) {
		this.Var = var;
	}
	public Boolean? Initialized { get; set; }
	public VariableDefinition Var { get; }
	public Instruction Instruction { get; set; }
}