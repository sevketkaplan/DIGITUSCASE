using Core.ResultType;
using DataObject.WM;

namespace Utility.Security.Jwt
{
    public interface ITokenHelper
    {
         Result<AccessToken> CreateToken(int id);
    }
}
