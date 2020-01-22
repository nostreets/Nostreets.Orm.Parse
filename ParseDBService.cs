using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data;
using System.Data.Common; 
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Nostreets.Extensions.Extend.Basic;
using Nostreets.Extensions.Extend.Data;
using Nostreets.Extensions.Interfaces;
using Parse;

namespace Nostreets.Orm.EF
{
    public class ParseDBService<T> : IDBService<T> where T : class
    {
        public ParseDBService(string appKey, string windowsKey = "")
        {
            ValidateType();

            ParseClient.Initialize(new ParseClient.Configuration
            {
                ApplicationId =  appKey,
                WindowsKey = windowsKey
            });
        }

        private void BackupDB(string path)
        {
           
        }

        private string GetPKName(Type type, out string output)
        {
            output = null;
            PropertyInfo pk = type.GetPropertiesByKeyAttribute()?.FirstOrDefault() ?? type.GetProperties()[0];

            if (!type.IsClass)
                output = "Generic Type has to be a custom class...";
            else if (type.IsSystemType())
                output = "Generic Type cannot be a system type...";
            else if (!pk.Name.ToLower().Contains("id") && !(pk.PropertyType == typeof(int) || pk.PropertyType == typeof(Guid) || pk.PropertyType == typeof(string)))
                output = "Primary Key must be the data type of Int32, Guid, or String and the Name needs ID in it...";

            return pk.Name;
        }

        private string GetTableName()
        {
            return typeof(T).Name;
        }

        private bool ValidateType() {

            GetPKName(typeof(T), out string output);

            if (output != null)
                throw new Exception(output);

            if (!typeof(T).IsCollection())
                throw new Exception("type of T needs not be a IEnumarable");

            return true;
        } 


        public int Count()
        {
            throw new NotImplementedException();
        }

        public List<T> GetAll()
        {
            throw new NotImplementedException();
        }

        public T Get(object id)
        {
            throw new NotImplementedException();
        }

        public T Get(object id, Converter<T, T> converter)
        {
            throw new NotImplementedException();
        }

        public object Insert(T model)
        {
            throw new NotImplementedException();
        }

        public object Insert(T model, Converter<T, T> converter)
        {


            ParseObject myCustomClass = new ParseObject(GetTableName());
            myCustomClass["myCustomKey1Name"] = "My custom value";
            myCustomClass["myCustomKey2Name"] = 999;



            await myCustomClass.SaveAsync();
        }

        public object[] Insert(IEnumerable<T> collection)
        {
            throw new NotImplementedException();
        }

        public object[] Insert(IEnumerable<T> collection, Converter<T, T> converter)
        {
            throw new NotImplementedException();
        }

        public void Update(IEnumerable<T> collection)
        {
            throw new NotImplementedException();
        }

        public void Update(IEnumerable<T> collection, Converter<T, T> converter)
        {
            throw new NotImplementedException();
        }

        public void Update(T model)
        {
            throw new NotImplementedException();
        }

        public void Update(T model, Converter<T, T> converter)
        {
            throw new NotImplementedException();
        }

        public void Delete(object id)
        {
            throw new NotImplementedException();
        }

        public void Delete(IEnumerable<object> ids)
        {
            throw new NotImplementedException();
        }

        public List<T> Where(Func<T, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public T FirstOrDefault(Func<T, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public void Backup(string disk = null)
        {
            throw new NotImplementedException();
        }

        public List<TResult> QueryResults<TResult>(string query, Dictionary<string, object> parameters = null)
        {
            throw new NotImplementedException();
        }
    }

}