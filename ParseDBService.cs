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
                ApplicationId = appKey,
                WindowsKey = windowsKey
            });
        }


        private string _pkName = null;
        private Type _type = null;


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

        private bool ValidateType()
        {

            _type = typeof(T);
            _pkName = GetPKName(_type, out string output);

            if (output != null)
                throw new Exception(output);

            if (_type.IsCollection())
                throw new Exception("type of T needs not be a IEnumarable");

            return true;
        }

        private T MapToT(ParseObject obj)
        {

            if (obj == null)
                throw new ArgumentNullException("obj");

            T result = null;

            foreach (var pair in obj)
            {
                if (result == null)
                    result.Instantiate();

                result.SetPropertyValue(pair.Key, pair.Value);
            }

            return result;

        }

        private ParseObject MapToParseObject(T obj, ParseObject parseObj = null)
        {

            if (obj == null)
                throw new ArgumentNullException("obj");

            ParseObject result = parseObj;
            Dictionary<string, object> dictionary = typeof(T).GetProperties().ToDictionary(p => p.Name, p => p.GetValue(obj, null));


            foreach (var pair in dictionary)
            {

                if (result == null)
                    result = new ParseObject(GetTableName());

                result[pair.Key] = pair.Value;
            }

            return result;
        }



        public int Count()
        {
            ParseQuery<ParseObject> query = ParseObject.GetQuery(GetTableName());
            return query.CountAsync().Result;
        }

        public List<T> GetAll()
        {
            List<T> result = null;
            ParseQuery<ParseObject> query = ParseObject.GetQuery(GetTableName());
            IEnumerable<ParseObject> list = query.FindAsync().Result;

            foreach (var obj in list)
            {
                if (result == null)
                    result = new List<T>();

                result.Add(MapToT(obj));
            }

            return result;
        }

        public T Get(object id)
        {
            string stringID = id == typeof(string) ? (string)id : id.ToString();
            ParseQuery<ParseObject> query = ParseObject.GetQuery(GetTableName());
            ParseObject obj = query.GetAsync(stringID).Result;

            T result = MapToT(obj);

            return result;
        }

        public T Get(object id, Converter<T, T> converter)
        {
            ParseQuery<ParseObject> query = ParseObject.GetQuery(GetTableName());
            ParseObject obj = query.GetAsync(id.ToString()).Result;

            T result = converter(MapToT(obj));

            return result;
        }

        public object Insert(T model)
        {
            PropertyInfo pk = model.GetType().GetProperty(_pkName);

            if (pk.PropertyType.Name.Contains("Int"))
                model.GetType().GetProperty(pk.Name).SetValue(model, Count() + 1);
            else if (pk.PropertyType.Name == "GUID")
                model.GetType().GetProperties().SetValue(Guid.NewGuid().ToString(), 0);

            ParseObject obj = MapToParseObject(model);

            obj.SaveAsync();

            return model.GetPropertyValue(_pkName);
        }

        public object Insert(T model, Converter<T, T> converter)
        {
            PropertyInfo pk = model.GetType().GetProperty(_pkName);

            if (pk.PropertyType.Name.Contains("Int"))
                model.GetType().GetProperty(pk.Name).SetValue(model, Count() + 1);
            else if (pk.PropertyType.Name == "GUID")
                model.GetType().GetProperties().SetValue(Guid.NewGuid().ToString(), 0);

            model = converter(model);

            ParseObject obj = MapToParseObject(model);

            obj.SaveAsync();

            return model.GetPropertyValue(_pkName);
        }

        public object[] Insert(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new NullReferenceException("collection");

            List<object> result = new List<object>();

            foreach (T item in collection)
                result.Add(Insert(item));

            return result.ToArray();
        }

        public object[] Insert(IEnumerable<T> collection, Converter<T, T> converter)
        {
            if (collection == null)
                throw new NullReferenceException("collection");

            if (converter == null)
                throw new NullReferenceException("converter");

            List<object> result = new List<object>();

            foreach (T item in collection)
                result.Add(Insert(item, converter));

            return result.ToArray();
        }

        public void Update(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new NullReferenceException("collection");

            foreach (T item in collection)
                 Update(item);
        }

        public void Update(IEnumerable<T> collection, Converter<T, T> converter)
        {
            if (collection == null)
                throw new NullReferenceException("collection");

            foreach (T item in collection)
                Update(converter(item));
        }

        public void Update(T model)
        {
            object id = model.GetPropertyValue(_pkName);
            string stringID = id == typeof(string) ? (string)id : id.ToString();

            ParseQuery <ParseObject> query = ParseObject.GetQuery(GetTableName());
            ParseObject obj = query.GetAsync(stringID).Result;

            obj = MapToParseObject(model, obj);

            obj.SaveAsync();
        }

        public void Update(T model, Converter<T, T> converter)
        {
            object id = model.GetPropertyValue(_pkName);
            string stringID = id == typeof(string) ? (string)id : id.ToString();

            ParseQuery<ParseObject> query = ParseObject.GetQuery(GetTableName());
            ParseObject obj = query.GetAsync(stringID).Result;

            model = converter(model);

            obj = MapToParseObject(model, obj);

            obj.SaveAsync();
        }

        public void Delete(object id)
        {
            string stringID = id == typeof(string) ? (string)id : id.ToString();

            ParseQuery<ParseObject> query = ParseObject.GetQuery(GetTableName());
            ParseObject obj = query.GetAsync(stringID).Result;

            obj.DeleteAsync();
        }

        public void Delete(IEnumerable<object> ids)
        {
            if (ids == null)
                throw new NullReferenceException("ids");

            foreach (T id in ids)
                Delete(id);
        }

        public List<T> Where(Func<T, bool> predicate)
        {
            List<T> result = null;
            ParseQuery<ParseObject> query = ParseObject.GetQuery(GetTableName());
            IEnumerable<ParseObject> list = query.Where(a => predicate(MapToT(a))).FindAsync().Result;

            foreach (var obj in list)
            {
                if (result == null)
                    result = new List<T>();

                result.Add(MapToT(obj));
            }

            return result;
        }

        public T FirstOrDefault(Func<T, bool> predicate)
        {
            T result = null;
            ParseQuery<ParseObject> query = ParseObject.GetQuery(GetTableName());
            //ParseObject list = query.FirstOrDefaultAsync(a => predicate(MapToT(a))).Result;
            result = Where(predicate).FirstOrDefault();

            return result;
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