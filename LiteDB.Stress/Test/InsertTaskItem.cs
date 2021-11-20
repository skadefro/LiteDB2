﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace LiteDB.Stress
{
    public class InsertTaskItem : ITestItem
    {
        private Random _rnd = new Random();

        private ILiteCollection<BsonDocument> _collection;

        public string Name { get; }
        public int TaskCount { get; }
        public TimeSpan Sleep { get; }
        public string Collection { get; }
        public BsonAutoId AutoId { get; }
        public int MinRange { get; }
        public int MaxRange { get; }
        public List<InsertField> Fields { get; }

        public InsertTaskItem(XmlElement el)
        {
            this.Name = string.IsNullOrEmpty(el.GetAttribute("name")) ? "INSERT_" + el.GetAttribute("collection").ToUpper() : el.GetAttribute("name");
            this.Sleep = string.IsNullOrEmpty(el.GetAttribute("sleep")) ? TimeSpan.FromSeconds(1) : TimeSpanEx.Parse(el.GetAttribute("sleep"));
            this.AutoId = string.IsNullOrEmpty(el.GetAttribute("autoId")) ? BsonAutoId.ObjectId : (BsonAutoId)Enum.Parse(typeof(BsonAutoId), el.GetAttribute("autoId"), true);
            this.Collection = el.GetAttribute("collection");
            this.TaskCount = string.IsNullOrEmpty(el.GetAttribute("tasks")) ? 1 : int.Parse(el.GetAttribute("tasks"));
            this.MinRange = string.IsNullOrEmpty(el.GetAttribute("docs")) ? 1 : int.Parse(el.GetAttribute("docs").Split('~').First());
            this.MaxRange = string.IsNullOrEmpty(el.GetAttribute("docs")) ? 1 : int.Parse(el.GetAttribute("docs").Split('~').Last());

            this.Fields = new List<InsertField>();

            foreach (XmlElement child in el.SelectNodes("*"))
            {
                this.Fields.Add(new InsertField(child));
            }
        }

        public BsonValue Execute(LiteDatabase db)
        {
            IEnumerable<BsonDocument> source()
            {
                var count = _rnd.Next(this.MinRange, this.MaxRange);

                for(var i = 0; i < count; i++)
                {
                    var doc = new BsonDocument();

                    foreach(var field in this.Fields)
                    {
                        doc[field.Name] = field.GetValue();
                    }

                    yield return doc;
                }
            }

            _collection ??= db.GetCollection(this.Collection, this.AutoId);

            return _collection.Insert(source());
        }
    }
}
