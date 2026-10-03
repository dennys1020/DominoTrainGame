using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoTrainGame.Utils
{
    public enum SignUpValidationStatus
    {
        Success,
        EmptyFields,
        InvalidEmail,
        PasswordTooShort,
        UserAlreadyExists,
        DatabaseError
    }

}
