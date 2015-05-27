using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToAnything.Results;
using System.Collections;

namespace LinqToSqlServer
{
    public class ParserResult
    {
        public string Sql { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }

    public static class StringFormatAppendExtension
    {
        public static StringBuilder AppendFormat(this StringBuilder sb, string format, params string[] prs)
        {
            return sb.Append(string.Format(format, prs));
        }
    }

    public class SqlServerQueryParser
    {
        private readonly QueryInfo _queryInfo;
        private readonly string _methodName;
        private readonly string _table;

        public SqlServerQueryParser(string table, QueryInfo queryInfo, string methodName = null)
        {
            _table = table;
            _queryInfo = queryInfo;
            _methodName = methodName ?? "Select";
        }

        public ParserResult Parse()
        {
            var param = new Dictionary<string, object>();
            var sql = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(_table))
            {
                switch (_methodName)
                {
                    case ("Select"):
                        sql.AppendFormat("SELECT * FROM [{0}] ", _table);
                        break;
                    default:
                        throw new Exception("Method " + _methodName + " not supported.");
                }
            }

            if (_queryInfo.Clauses.Any())
            {
                sql.Append(" WHERE ");
                var clauses = _queryInfo.Clauses.ToArray();
                var count = clauses.Length;
                for (int i = 0; i < count; i++)
                {
                    if (i > 0) sql.Append(" AND ");
                    if (count > 1) sql.Append("(");
                    Parse(clauses[i], sql, param);

                    if (count > 1) sql.Append("(");
                }
            }

            if (_queryInfo.OrderBys.Any())
            {
                sql.Append(" ORDER BY ");
                var clauses = _queryInfo.OrderBys.ToArray();
                var count = clauses.Length;
                for (int i = 0; i < count; i++)
                {
                    if (i > 0) sql.Append(", ");
                    Parse(clauses[i], sql, param);
                }
            }

            if (_queryInfo.Skip > 0)
            {
                sql.AppendFormat(" OFFSET {0} ROWS ", _queryInfo.Skip);
            }

            if (_queryInfo.Take != null && _queryInfo.Take.Value > 0)
            {
                sql.AppendFormat(" FETCH NEXT {0} ROWS ONLY ", _queryInfo.Take.Value);
            }

            return new ParserResult
            {
                Sql = sql.ToString(),
                Parameters = param
            };
        }

        private void Parse(OrderBy orderBy, StringBuilder sql, Dictionary<string, object> param)
        {
            sql.AppendFormat("[{0}]", orderBy.Name);
            if (orderBy.Direction == OrderBy.OrderByDirection.Asc)
            {
                sql.Append(" ASC ");
            }
            else
            {
                sql.Append(" DESC ");
            }
        }

        private void ParseBinary(BinaryOperator binaryOperator, StringBuilder sb, Dictionary<string, object> par)
        {
            switch (binaryOperator.Operator)
            {
                case ("Equal"):
                    Parse(binaryOperator.Parameters[0], sb, par);
                    sb.Append("=");
                    Parse(binaryOperator.Parameters[1], sb, par);
                    break;
                case ("NotEqual"):
                    Parse(binaryOperator.Parameters[0], sb, par);
                    sb.Append("<>");
                    Parse(binaryOperator.Parameters[1], sb, par);
                    break;
                default:
                    throw new Exception("BinaryOperator " + binaryOperator.Operator + " not supported.");

            }
        }

        private void ParseConstant(Constant constant, StringBuilder sb, Dictionary<string, object> par)
        {
            var parName = "p_" + par.Count;
            sb.AppendFormat("@{0}", parName);
            par.Add(parName, constant.Value);
        }

        private void ParseMember(Member member, StringBuilder sb, Dictionary<string, object> par)
        {
            sb.AppendFormat("[{0}]", member.Name);
        }

        private void ParseWhere(Where where, StringBuilder sb, Dictionary<string, object> par)
        {

            switch (where.Operator)
            {
                case ("Contains"):
                    {
                        var constant = where.Parameters[1] as Constant;
                        if (constant != null)
                        {
                            if (constant.Value != null && constant.Value.GetType() != typeof(string) && constant.Value is IEnumerable)
                            {
                                var parName = "p_" + par.Count;
                                Parse(where.Parameters[0], sb, par);

                                sb.Append(" IN ");
                                sb.AppendFormat("@{0}", parName);
                                par.Add(parName, constant.Value);
                                break;
                            }
                            else if (constant.Value != null && (constant.Value.GetType() == typeof(string) || constant.Value.GetType() == typeof(char)))
                            {
                                var parName = "p_" + par.Count;
                                Parse(where.Parameters[0], sb, par);

                                sb.Append(" LIKE ");
                                sb.AppendFormat("@{0}", parName);
                                par.Add(parName, "%" + constant.Value + "%");
                                break;
                            }
                        }
                        Parse(where.Parameters[0], sb, par);
                        sb.Append("=");
                        Parse(where.Parameters[1], sb, par);

                    }
                    break;

                default:
                    throw new Exception("Where " + where.Operator + " not supported.");
            }
        }


        private void ParseCall(Call call, StringBuilder sb, Dictionary<string, object> par)
        {

            switch (call.Method)
            {
                case ("ToLower"):
                    sb.Append("LOWER(");
                    Parse(call.Parameters[0], sb, par);
                    sb.Append(")");
                    break;
                case ("ToUpper"):
                    sb.Append("UPPER(");
                    Parse(call.Parameters[0], sb, par);
                    sb.Append(")");
                    break;
                default:
                    throw new Exception("Call " + call.Method + " not supported.");
            }
        }

        public void Parse(Clause clause, StringBuilder sb, Dictionary<string, object> par)
        {
            switch (clause.GetType().Name)
            {
                case ("BinaryOperator"):
                    ParseBinary((BinaryOperator)clause, sb, par);
                    break;
                case ("Member"):
                    ParseMember((Member)clause, sb, par);
                    break;
                case ("Constant"):
                    ParseConstant((Constant)clause, sb, par);
                    break;
                case ("Where"):
                    ParseWhere((Where)clause, sb, par);
                    break;
                case ("Call"):
                    ParseCall((Call)clause, sb, par);
                    break;
                case ("AndOr"):
                    ParseAndOr((AndOr)clause, sb, par);
                    break;
                case ("OrderBy"):
                default:
                    throw new Exception("Clause " + clause.GetType().Name + " not supported.");
            }
        }

        private void ParseAndOr(AndOr andOr, StringBuilder sb, Dictionary<string, object> par)
        {
            switch (andOr.Operator)
            {
                case ("AND"):
                    sb.Append("((");
                    Parse(andOr.Parameters[0], sb, par);
                    sb.Append(") AND (");
                    Parse(andOr.Parameters[1], sb, par);
                    sb.Append("))");
                    break;
                case ("OR"):
                    sb.Append("((");
                    Parse(andOr.Parameters[0], sb, par);
                    sb.Append(") OR (");
                    Parse(andOr.Parameters[1], sb, par);
                    sb.Append("))");
                    break;
                default:
                    throw new Exception("AndOr " + andOr.Operator + " not supported.");

            }
        }
    }
}
