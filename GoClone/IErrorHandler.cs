using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone;
internal interface IErrorHandler
{
    void ReportError(Token token, string message);
    void ReportWarning(Token token, string message);
    void ReportNotice(Token token, string message);
}
