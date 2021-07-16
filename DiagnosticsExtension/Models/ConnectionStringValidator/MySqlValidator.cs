﻿using DiagnosticsExtension.Controllers;
using DiagnosticsExtension.Models.ConnectionStringValidator.Exceptions;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace DiagnosticsExtension.Models.ConnectionStringValidator
{
    public class MySqlValidator : IConnectionStringValidator
    {
        public string ProviderName => "MySql.Data.MySqlClient";

        public ConnectionStringType Type => ConnectionStringType.MySql;

        public bool IsValid(string connStr)
        {
            try
            {
                var builder = new MySqlConnectionStringBuilder(connStr);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        async public Task<ConnectionStringValidationResult> Validate(string connStr, string clientId = null)
        {
            var response = new ConnectionStringValidationResult(Type);

            try
            {
                try
                {
                    var builder = new MySqlConnectionStringBuilder(connStr);
                }
                catch (Exception e)
                {
                    throw new MalformedConnectionStringException(e.Message, e);
                }

                var result = await DatabaseTestController.TestMySqlConnectionString(connStr, null, clientId);
                if (result.Succeeded)
                {
                    response.Status = ConnectionStringValidationResult.ResultStatus.Success;
                }
                else
                {
                    throw new Exception("Unexpected state reached: result.Succeeded == false is unexpected!");
                }
            }
            catch (Exception e)
            {
                if (e is MalformedConnectionStringException)
                {
                    response.Status = ConnectionStringValidationResult.ResultStatus.MalformedConnectionString;
                }
                else if (e is MySqlException)
                {
                    if (e.Message.StartsWith("Unable to connect to any of the specified MySQL hosts."))
                    {
                        response.Status = ConnectionStringValidationResult.ResultStatus.EndpointNotReachable;
                    }
                    else if (e.Message.Contains("not allowed"))
                    {
                        response.Status = ConnectionStringValidationResult.ResultStatus.Forbidden;
                    }
                    else if (e.Message.StartsWith("Authentication to host"))
                    {
                        response.Status = ConnectionStringValidationResult.ResultStatus.AuthFailure;
                    }
                    else
                    {
                        response.Status = ConnectionStringValidationResult.ResultStatus.UnknownError;
                    }
                }
                else
                {
                    response.Status = ConnectionStringValidationResult.ResultStatus.UnknownError;
                }
                response.Exception = e;
            }

            return response;
        }
    }
}