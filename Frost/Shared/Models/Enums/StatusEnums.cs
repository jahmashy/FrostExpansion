using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Shared.Models.Enums
{
    public enum FileStatusCode
    {
        FileDoesNotExists,
        NotEnoughFiles,
        TooManyFiles,
        FileIsTooBig,
        FileIsNull,
        InvalidFileExtension,
        Success
    }
}
