using System;
using System.Collections.Generic;
using System.Text;

namespace MongoDB.Crypt
{
    internal interface IStatus
    {
        void Check(Status status);
    }
}
