using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone;
internal class ExceptionErrorHandler : IErrorHandler
{
    public void ReportError(Token token, string message)
    {
        throw new Exception($"ERROR on char {token.start}: {message}");
    }

    public void ReportNotice(Token token, string message)
    {
        throw new NotImplementedException();
    }

    public void ReportWarning(Token token, string message)
    {
        throw new NotImplementedException();
    }
}
