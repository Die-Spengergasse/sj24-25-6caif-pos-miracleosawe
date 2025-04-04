using System;

namespace SPG_Fachtheorie.Aufgabe1.Services
{
    [Serializable]
    public class EmployeeServiceException : Exception
    {
        public bool IsNotFoundError { get; set; }
        public EmployeeServiceException()
        {
        }

        public EmployeeServiceException(
            string? message, bool isNotFoundError = false) : base(message)
        {
            IsNotFoundError = isNotFoundError;
        }

        public EmployeeServiceException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}