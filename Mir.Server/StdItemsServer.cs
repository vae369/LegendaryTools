﻿using Mir.IService;
using Mir.Models;
using Mir.ORM.SqlSugar.Repostitory;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mir.Server
{
    public class StdItemsServer : Repostitory<StdItems>, IStdItemsService
    {
        public StdItemsServer(SqlSugar.DbType dbt) : base(dbt)
        {
        }

        public System.Data.DataTable GetStdItemsByTable(string where)
        {
            DataTable dt = new DataTable();
            try
            {
                string sql = "select * from StdItems where 1=1 ";
                if (!string.IsNullOrEmpty(where))
                    sql += where;
                sql += " order by idx asc";
                dt = Context.Ado.GetDataTable(sql);
            }
            catch (Exception ex)
            {
            }

            return dt;
        }

        public bool SaveByStorageable(DataTable dt)
        {
            string sql = "";
            string insertColumn = "";
            string insertRowData = "";
            foreach (DataRow dr in dt.Rows)
            {
                switch (dr.RowState)
                {
                    case DataRowState.Modified:
                        {
                            foreach (DataColumn dc in dt.Columns)
                            {
                                //自定义的最大生命值字段，所以不需要添加数据到数据库中
                                if (dc.ColumnName.ToLower() == "maxhp") continue;
                                if (dc.ColumnName.ToLower() == "m_id") continue;

                                // 使用TypeCode进行判断
                                TypeCode typeCode = Type.GetTypeCode(dc.DataType);
                                switch (typeCode)
                                {
                                    case TypeCode.Int32:
                                        if (string.IsNullOrEmpty(dr[dc.ColumnName].ToString()))
                                        {
                                            insertRowData += $"{dc.ColumnName}=0,";
                                        }
                                        else
                                        {
                                            insertRowData += $"{dc.ColumnName}={dr[dc.ColumnName]},";
                                        }
                                        break;
                                    case TypeCode.String: insertColumn = $"{dc.ColumnName}='{dr[dc.ColumnName]}',"; break;
                                    default:
                                        if (string.IsNullOrEmpty(dr[dc.ColumnName].ToString()))
                                        {
                                            insertRowData += $"{dc.ColumnName}=0,";
                                        }
                                        else
                                        {
                                            insertRowData += $"{dc.ColumnName}={dr[dc.ColumnName]},";
                                        }
                                        break;
                                }
                            }
                            insertColumn = insertColumn.TrimEnd(',');
                            insertRowData = insertRowData.TrimEnd(',');
                            sql = $"update StdItems set {insertRowData} where {insertColumn}";
                            Context.Ado.ExecuteCommand(sql);
                        }
                        break;
                    case DataRowState.Deleted: break;
                    case DataRowState.Added:
                        {
                            foreach (DataColumn dc in dt.Columns)
                            {
                                //自定义的最大生命值字段，所以不需要添加数据到数据库中
                                if (dc.ColumnName.ToLower() == "maxhp") continue;
                                if (dc.ColumnName.ToLower() == "m_id") continue;
                                insertColumn += dc.ColumnName + ",";
                                TypeCode typeCode = Type.GetTypeCode(dc.DataType);
                                switch (typeCode)
                                {
                                    case TypeCode.Int32:
                                        if (string.IsNullOrEmpty(dr[dc.ColumnName].ToString()))
                                        {
                                            insertRowData += "0,";
                                        }
                                        else
                                        {
                                            insertRowData += dr[dc.ColumnName] + ",";
                                        }
                                        break;
                                    case TypeCode.String: insertRowData += $"'{dr[dc.ColumnName]}',"; break;
                                    default:
                                        if (string.IsNullOrEmpty(dr[dc.ColumnName].ToString()))
                                        {
                                            insertRowData += "0,";
                                        }
                                        else
                                        {
                                            insertRowData += dr[dc.ColumnName] + ",";
                                        }
                                        break;
                                }
                            }

                            insertColumn = insertColumn.TrimEnd(',');
                            insertRowData = insertRowData.TrimEnd(',');
                            sql = $"insert into StdItems({insertColumn})values({insertRowData})";
                            Context.Ado.ExecuteCommand(sql);
                        }
                        break;
                }

            }

            return true;
        }

        public bool SaveSingle(string tableName, Dictionary<string, string> SqlColmun)
        {
            bool flag = false;
            if (SqlColmun.Count >= 4)
            {
                string sql = $"update {tableName} set {SqlColmun["SaveKey"]}={SqlColmun["SaveValue"]} where {SqlColmun["PrimaryKey"]}={SqlColmun["PrimaryValue"]}";
                if (Context.Ado.ExecuteCommand(sql) > 0) flag = true;
            }
            return flag;
        }

        public void Delete(List<string> ids)
        {
            foreach (string id in ids)
            {
                string sql = "delete from StdItems where Idx=" + id;
                int num = Context.Ado.ExecuteCommand(sql);
            }
        }
    }
}
