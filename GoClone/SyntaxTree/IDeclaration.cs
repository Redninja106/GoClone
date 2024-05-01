using GoClone.CodeGeneration;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree;
internal interface IDeclaration
{
    void Register(ModuleScope scope);
    void Resolve(ModuleScope scope);
    void Emit(ModuleScope scope);
}

/*
 * modA
 * type A struct {  }
 * type I1 interface { func i1() }
 *  
 *
 * modB
 * 
 * type I2 interface { func i2() }
 * 
 * func main() { 
 * }
 */
