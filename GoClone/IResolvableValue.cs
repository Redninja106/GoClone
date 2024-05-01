using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone;
internal interface IResolvableValue
{
    LLVMValueRef Value { get; }
}
