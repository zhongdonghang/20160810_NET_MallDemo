using System;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Collections.Generic;

using BrnMall.Core;

namespace BrnMall.SearchStrategy.SqlServer
{
    /// <summary>
    /// 基于SqlServer的搜索策略
    /// </summary>
    public partial class SearchStrategy : ISearchStrategy
    {
        /// <summary>
        /// 搜索商城商品
        /// </summary>
        /// <param name="pageSize">每页数</param>
        /// <param name="pageNumber">当前页数</param>
        /// <param name="keyword">关键词</param>
        /// <param name="cateId">分类id</param>
        /// <param name="brandId">品牌id</param>
        /// <param name="filterPrice">筛选价格</param>
        /// <param name="catePriceRangeList">分类价格范围列表</param>
        /// <param name="attrValueIdList">属性值id列表</param>
        /// <param name="onlyStock">是否只显示有货</param>
        /// <param name="sortColumn">排序列</param>
        /// <param name="sortDirection">排序方向</param>
        /// <returns></returns>
        public List<StoreProductInfo> SearchMallProducts(int pageSize, int pageNumber, string keyword, int cateId, int brandId, int filterPrice, string[] catePriceRangeList, List<int> attrValueIdList, int onlyStock, int sortColumn, int sortDirection)
        {
            StringBuilder commandText = new StringBuilder();

            if (pageNumber == 1)
            {
                commandText.AppendFormat("SELECT distinct TOP {1} [p].[pid],[p].[psn],[p].[cateid],[p].[brandid],[p].[storeid],[p].[storecid],[p].[storestid],[p].[skugid],[p].[name],[p].[shopprice],[p].[marketprice],[p].[costprice],[p].[state],[p].[isbest],[p].[ishot],[p].[isnew],[p].[displayorder],[p].[weight],[p].[showimg],[p].[salecount],[p].[visitcount],[p].[reviewcount],[p].[star1],[p].[star2],[p].[star3],[p].[star4],[p].[star5],[p].[addtime],[pk].[relevancy],[s].[name] AS [storename] FROM [{0}products] AS [p]", RDBSHelper.RDBSTablePre, pageSize);

                if (onlyStock == 1)
                    commandText.AppendFormat(" LEFT JOIN [{0}productstocks] AS [ps] ON [p].[pid]=[ps].[pid]", RDBSHelper.RDBSTablePre);

                commandText.AppendFormat(" LEFT JOIN [{0}productkeywords] AS [pk] ON [p].[pid]=[pk].[pid]", RDBSHelper.RDBSTablePre);

                commandText.AppendFormat(" LEFT JOIN [{0}stores] AS [s] ON [p].[storeid]=[s].[storeid] WHERE", RDBSHelper.RDBSTablePre);

                if (cateId > 0)
                {
                    StringBuilder cateIds = GetAllCateIdsByParentCateId(cateId);
                    commandText.AppendFormat("[p].[cateid] in ({0}) AND", cateIds);
                }

                if (brandId > 0)
                {
                    commandText.AppendFormat("[p].[brandid]={0} AND", brandId);
                }

                if (filterPrice > 0 && filterPrice <= catePriceRangeList.Length)
                {
                    string[] priceRange = StringHelper.SplitString(catePriceRangeList[filterPrice - 1], "-");
                    if (priceRange.Length == 1)
                        commandText.AppendFormat(" [p].[shopprice]>='{0}' AND", priceRange[0]);
                    else if (priceRange.Length == 2)
                        commandText.AppendFormat(" [p].[shopprice]>='{0}' AND [p].[shopprice]<'{1}' AND", priceRange[0], priceRange[1]);
                }

                commandText.Append(" [p].[state]=0");

                if (attrValueIdList!=null && attrValueIdList.Count > 0)
                {
                    commandText.Append(" AND [p].[pid] IN (SELECT [pa1].[pid] FROM");
                    for (int i = 0; i < attrValueIdList.Count; i++)
                    {
                        if (i == 0)
                            commandText.AppendFormat(" (SELECT [pid] FROM [{0}productattributes] WHERE [attrvalueid]={1}) AS [pa1]", RDBSHelper.RDBSTablePre, attrValueIdList[i]);
                        else
                            commandText.AppendFormat(" INNER JOIN (SELECT [pid] FROM [{0}productattributes] WHERE [attrvalueid]={1}) AS [pa{2}] ON [pa{2}].[pid]=[pa{3}].[pid]", RDBSHelper.RDBSTablePre, attrValueIdList[i], i + 1, i);
                    }
                    commandText.Append(")");
                }

                if (onlyStock == 1)
                    commandText.Append(" AND [ps].[number]>0");

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    commandText.AppendFormat(" AND ([pk].[keyword]='{0}' OR [p].[name] like('%{0}%'))", keyword);
                }

                commandText.Append(" AND [s].[state]=0");

                commandText.Append(" ORDER BY ");
                switch (sortColumn)
                {
                    case 0:
                        commandText.Append("[pk].[relevancy]");
                        break;
                    case 1:
                        commandText.Append("[p].[salecount]");
                        break;
                    case 2:
                        commandText.Append("[p].[shopprice]");
                        break;
                    case 3:
                        commandText.Append("[p].[reviewcount]");
                        break;
                    case 4:
                        commandText.Append("[p].[addtime]");
                        break;
                    case 5:
                        commandText.Append("[p].[visitcount]");
                        break;
                    default:
                        commandText.Append("[pk].[relevancy]");
                        break;
                }
                switch (sortDirection)
                {
                    case 0:
                        commandText.Append(" DESC");
                        break;
                    case 1:
                        commandText.Append(" ASC");
                        break;
                    default:
                        commandText.Append(" DESC");
                        break;
                }
            }
            else
            {
                commandText.Append("SELECT distinct [pid],[psn],[cateid],[brandid],[storeid],[storecid],[storestid],[skugid],[name],[shopprice],[marketprice],[costprice],[state],[isbest],[ishot],[isnew],[displayorder],[weight],[showimg],[salecount],[visitcount],[reviewcount],[star1],[star2],[star3],[star4],[star5],[addtime],[storename] FROM");
                commandText.Append(" (SELECT ROW_NUMBER() OVER (ORDER BY ");
                switch (sortColumn)
                {
                    case 0:
                        commandText.Append("[temp].[relevancy]");
                        break;
                    case 1:
                        commandText.Append("[temp].[salecount]");
                        break;
                    case 2:
                        commandText.Append("[temp].[shopprice]");
                        break;
                    case 3:
                        commandText.Append("[temp].[reviewcount]");
                        break;
                    case 4:
                        commandText.Append("[temp].[addtime]");
                        break;
                    case 5:
                        commandText.Append("[temp].[visitcount]");
                        break;
                    default:
                        commandText.Append("[temp].[relevancy]");
                        break;
                }
                switch (sortDirection)
                {
                    case 0:
                        commandText.Append(" DESC");
                        break;
                    case 1:
                        commandText.Append(" ASC");
                        break;
                    default:
                        commandText.Append(" DESC");
                        break;
                }
                commandText.AppendFormat(") AS [rowid],[temp].[pid],[temp].[psn],[temp].[cateid],[temp].[brandid],[temp].[storeid],[temp].[storecid],[temp].[storestid],[temp].[skugid],[temp].[name],[temp].[shopprice],[temp].[marketprice],[temp].[costprice],[temp].[state],[temp].[isbest],[temp].[ishot],[temp].[isnew],[temp].[displayorder],[temp].[weight],[temp].[showimg],[temp].[salecount],[temp].[visitcount],[temp].[reviewcount],[temp].[star1],[temp].[star2],[temp].[star3],[temp].[star4],[temp].[star5],[temp].[addtime],[temp].[storename] from (select distinct [p].[pid],[p].[psn],[p].[cateid],[p].[brandid],[p].[storeid],[p].[storecid], [p].[storestid],[p].[skugid],[p].[name],[p].[shopprice],[p].[marketprice],[p].[costprice],[p].[state],[p].[isbest],[p].[ishot],[p].[isnew],[p].[displayorder],[p].[weight],[p].[showimg],[p].[salecount],[p].[visitcount],[p].[reviewcount],[p].[star1],[p].[star2],[p].[star3],[p].[star4],[p].[star5],[p].[addtime],[s].[name] as [storename] ,[pk].[relevancy] as [relevancy] FROM [{0}products] AS [p]", RDBSHelper.RDBSTablePre);

                if (onlyStock == 1)
                    commandText.AppendFormat(" LEFT JOIN [{0}productstocks] AS [ps] ON [p].[pid]=[ps].[pid]", RDBSHelper.RDBSTablePre);

                commandText.AppendFormat(" LEFT JOIN [{0}productkeywords] AS [pk] ON [p].[pid]=[pk].[pid]", RDBSHelper.RDBSTablePre);

                commandText.AppendFormat(" LEFT JOIN [{0}stores] AS [s] ON [p].[storeid]=[s].[storeid] WHERE", RDBSHelper.RDBSTablePre);
                if (cateId > 0)
                {
                    StringBuilder cateIds = GetAllCateIdsByParentCateId(cateId);
                    commandText.AppendFormat("[p].[cateid] in ({0}) AND", cateIds);
                }

                if (brandId > 0)
                {
                    commandText.AppendFormat("[p].[brandid]={0} AND", brandId);
                }

                if (filterPrice > 0 && filterPrice <= catePriceRangeList.Length)
                {
                    string[] priceRange = StringHelper.SplitString(catePriceRangeList[filterPrice - 1], "-");
                    if (priceRange.Length == 1)
                        commandText.AppendFormat(" [p].[shopprice]>='{0}' AND", priceRange[0]);
                    else if (priceRange.Length == 2)
                        commandText.AppendFormat(" [p].[shopprice]>='{0}' AND [p].[shopprice]<'{1}' AND", priceRange[0], priceRange[1]);
                }

                commandText.Append(" [p].[state]=0");

                if (attrValueIdList !=null && attrValueIdList.Count > 0)
                {
                    commandText.Append(" AND [p].[pid] IN (SELECT [pa1].[pid] FROM");
                    for (int i = 0; i < attrValueIdList.Count; i++)
                    {
                        if (i == 0)
                            commandText.AppendFormat(" (SELECT [pid] FROM [{0}productattributes] WHERE [attrvalueid]={1}) AS [pa1]", RDBSHelper.RDBSTablePre, attrValueIdList[i]);
                        else
                            commandText.AppendFormat(" INNER JOIN (SELECT [pid] FROM [{0}productattributes] WHERE [attrvalueid]={1}) AS [pa{2}] ON [pa{2}].[pid]=[pa{3}].[pid]", RDBSHelper.RDBSTablePre, attrValueIdList[i], i + 1, i);
                    }
                    commandText.Append(")");
                }

                if (onlyStock == 1)
                    commandText.Append(" AND [ps].[number]>0");

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    commandText.AppendFormat(" AND [pk].[keyword]='{0}' OR [p].[name] like('%{0}%')", keyword);
                }

                commandText.Append(" AND [s].[state]=0");

                commandText.Append(") AS [temp]"); 
                
                commandText.Append(") AS [temp1]");
                commandText.AppendFormat(" WHERE [rowid] BETWEEN {0} AND {1}", pageSize * (pageNumber - 1) + 1, pageSize * pageNumber);

            }

            List<StoreProductInfo> storeProductList = new List<StoreProductInfo>();
            IDataReader reader = RDBSHelper.ExecuteReader(CommandType.Text, commandText.ToString());
            while (reader.Read())
            {
                StoreProductInfo storeProductInfo = new StoreProductInfo();

                storeProductInfo.Pid = TypeHelper.ObjectToInt(reader["pid"]);
                storeProductInfo.PSN = reader["psn"].ToString();
                storeProductInfo.CateId = TypeHelper.ObjectToInt(reader["cateid"]);
                storeProductInfo.BrandId = TypeHelper.ObjectToInt(reader["brandid"]);
                storeProductInfo.StoreId = TypeHelper.ObjectToInt(reader["storeid"]);
                storeProductInfo.StoreCid = TypeHelper.ObjectToInt(reader["storecid"]);
                storeProductInfo.StoreSTid = TypeHelper.ObjectToInt(reader["storestid"]);
                storeProductInfo.SKUGid = TypeHelper.ObjectToInt(reader["skugid"]);
                storeProductInfo.Name = reader["name"].ToString();
                storeProductInfo.ShopPrice = TypeHelper.ObjectToDecimal(reader["shopprice"]);
                storeProductInfo.MarketPrice = TypeHelper.ObjectToDecimal(reader["marketprice"]);
                storeProductInfo.CostPrice = TypeHelper.ObjectToDecimal(reader["costprice"]);
                storeProductInfo.State = TypeHelper.ObjectToInt(reader["state"]);
                storeProductInfo.IsBest = TypeHelper.ObjectToInt(reader["isbest"]);
                storeProductInfo.IsHot = TypeHelper.ObjectToInt(reader["ishot"]);
                storeProductInfo.IsNew = TypeHelper.ObjectToInt(reader["isnew"]);
                storeProductInfo.DisplayOrder = TypeHelper.ObjectToInt(reader["displayorder"]);
                storeProductInfo.Weight = TypeHelper.ObjectToInt(reader["weight"]);
                storeProductInfo.ShowImg = reader["showimg"].ToString();
                storeProductInfo.SaleCount = TypeHelper.ObjectToInt(reader["salecount"]);
                storeProductInfo.VisitCount = TypeHelper.ObjectToInt(reader["visitcount"]);
                storeProductInfo.ReviewCount = TypeHelper.ObjectToInt(reader["reviewcount"]);
                storeProductInfo.Star1 = TypeHelper.ObjectToInt(reader["star1"]);
                storeProductInfo.Star2 = TypeHelper.ObjectToInt(reader["star2"]);
                storeProductInfo.Star3 = TypeHelper.ObjectToInt(reader["star3"]);
                storeProductInfo.Star4 = TypeHelper.ObjectToInt(reader["star4"]);
                storeProductInfo.Star5 = TypeHelper.ObjectToInt(reader["star5"]);
                storeProductInfo.AddTime = TypeHelper.ObjectToDateTime(reader["addtime"]);
                storeProductInfo.StoreName = reader["storename"].ToString();

                storeProductList.Add(storeProductInfo);
            }
            reader.Close();
            return storeProductList;
        }

        /// <summary>
        /// 获得搜索商城商品数量
        /// </summary>
        /// <param name="keyword">关键词</param>
        /// <param name="cateId">分类id</param>
        /// <param name="brandId">品牌id</param>
        /// <param name="filterPrice">筛选价格</param>
        /// <param name="catePriceRangeList">分类价格范围列表</param>
        /// <param name="attrValueIdList">属性值id列表</param>
        /// <param name="onlyStock">是否只显示有货</param>
        /// <returns></returns>
        public int GetSearchMallProductCount(string keyword, int cateId, int brandId, int filterPrice, string[] catePriceRangeList, List<int> attrValueIdList, int onlyStock)
        {
            StringBuilder commandText = new StringBuilder();

            commandText.AppendFormat("SELECT COUNT([temp].[pid]) FROM (SELECT distinct [p].[pid] FROM [{0}products] AS [p]", RDBSHelper.RDBSTablePre);

            if (onlyStock == 1)
                commandText.AppendFormat(" LEFT JOIN [{0}productstocks] AS [ps] ON [p].[pid]=[ps].[pid]", RDBSHelper.RDBSTablePre);

            commandText.AppendFormat(" LEFT JOIN [{0}productkeywords] AS [pk] ON [p].[pid]=[pk].[pid]", RDBSHelper.RDBSTablePre);

            commandText.AppendFormat(" LEFT JOIN [{0}stores] AS [s] ON [p].[storeid]=[s].[storeid] WHERE", RDBSHelper.RDBSTablePre);

            if (cateId > 0)
            {
                StringBuilder cateIds = GetAllCateIdsByParentCateId(cateId);
                commandText.AppendFormat(" [p].[cateid] in ({0}) AND", cateIds);
            }

            if (brandId > 0)
            {
                commandText.AppendFormat("[p].[brandid]={0} AND", brandId);
            }

            if (filterPrice > 0 && filterPrice <= catePriceRangeList.Length)
            {
                string[] priceRange = StringHelper.SplitString(catePriceRangeList[filterPrice - 1], "-");
                if (priceRange.Length == 1)
                    commandText.AppendFormat("[p].[shopprice]>='{0}' AND", priceRange[0]);
                else if (priceRange.Length == 2)
                    commandText.AppendFormat(" [p].[shopprice]>='{0}' AND [p].[shopprice]<'{1}' AND", priceRange[0], priceRange[1]);
            }

            commandText.Append(" [p].[state]=0");

            if (attrValueIdList != null && attrValueIdList.Count > 0)
            {
                commandText.Append(" AND [p].[pid] IN (SELECT [pa1].[pid] FROM");
                for (int i = 0; i < attrValueIdList.Count; i++)
                {
                    if (i == 0)
                        commandText.AppendFormat(" (SELECT [pid] FROM [{0}productattributes] WHERE [attrvalueid]={1}) AS [pa1]", RDBSHelper.RDBSTablePre, attrValueIdList[i]);
                    else
                        commandText.AppendFormat(" INNER JOIN (SELECT [pid] FROM [{0}productattributes] WHERE [attrvalueid]={1}) AS [pa{2}] ON [pa{2}].[pid]=[pa{3}].[pid]", RDBSHelper.RDBSTablePre, attrValueIdList[i], i + 1, i);
                }
                commandText.Append(")");
            }

            if (onlyStock == 1)
                commandText.Append(" AND [ps].[number]>0");

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                commandText.AppendFormat(" AND ([p].[name] like '%'+'{0}'+'%' OR [pk].[keyword]='{0}')", keyword);
            }

            commandText.Append(" AND [s].[state]=0");

            commandText.Append(" ) AS temp");
           
            return TypeHelper.ObjectToInt(RDBSHelper.ExecuteScalar(CommandType.Text, commandText.ToString()));
        }

        /// <summary>
        /// 搜索店铺商品
        /// </summary>
        /// <param name="pageSize">每页数</param>
        /// <param name="pageNumber">当前页数</param>
        /// <param name="keyword">关键词</param>
        /// <param name="storeId">店铺id</param>
        /// <param name="storeCid">店铺分类id</param>
        /// <param name="startPrice">开始价格</param>
        /// <param name="endPrice">结束价格</param>
        /// <param name="sortColumn">排序列</param>
        /// <param name="sortDirection">排序方向</param>
        /// <returns></returns>
        public List<PartProductInfo> SearchStoreProducts(int pageSize, int pageNumber, string keyword, int storeId, int storeCid, int startPrice, int endPrice, int sortColumn, int sortDirection)
        {
            StringBuilder commandText = new StringBuilder();

            if (pageNumber == 1)
            {
                commandText.AppendFormat("SELECT DISTINCT TOP {1} [p].[pid],[p].[psn],[p].[cateid],[p].[brandid],[p].[storeid],[p].[storecid],[p].[storestid],[p].[skugid],[p].[name],[p].[shopprice],[p].[marketprice],[p].[costprice],[p].[state],[p].[isbest],[p].[ishot],[p].[isnew],[p].[displayorder],[p].[weight],[p].[showimg],[p].[salecount],[p].[visitcount],[p].[reviewcount],[p].[star1],[p].[star2],[p].[star3],[p].[star4],[p].[star5],[p].[addtime],[pk].[relevancy] FROM [{0}products] AS [p]", RDBSHelper.RDBSTablePre, pageSize);

                commandText.AppendFormat(" LEFT JOIN [{0}productkeywords] AS [pk] ON [p].[pid]=[pk].[pid]", RDBSHelper.RDBSTablePre);

                commandText.AppendFormat(" WHERE [p].[storeid]={0}", storeId);

                if (storeCid > 0)
                    commandText.AppendFormat(" AND [p].[storecid]={0}", storeCid);

                if (startPrice > 0)
                    commandText.AppendFormat(" AND [p].[shopprice]>={0}", startPrice);
                if (endPrice > 0)
                    commandText.AppendFormat(" AND [p].[shopprice]<={0}", endPrice);

                commandText.Append(" AND [p].[state]=0");

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    commandText.AppendFormat(" AND( [p].[name] like '%{0}%' OR [pk].[keyword]='{0}')", keyword);
                }

                commandText.Append(" ORDER BY ");
                switch (sortColumn)
                {
                    case 0:
                        commandText.Append("[pk].[relevancy]");
                        break;
                    case 1:
                        commandText.Append("[p].[salecount]");
                        break;
                    case 2:
                        commandText.Append("[p].[shopprice]");
                        break;
                    case 3:
                        commandText.Append("[p].[reviewcount]");
                        break;
                    case 4:
                        commandText.Append("[p].[addtime]");
                        break;
                    case 5:
                        commandText.Append("[p].[visitcount]");
                        break;
                    default:
                        commandText.Append("[pk].[relevancy]");
                        break;
                }
                switch (sortDirection)
                {
                    case 0:
                        commandText.Append(" DESC");
                        break;
                    case 1:
                        commandText.Append(" ASC");
                        break;
                    default:
                        commandText.Append(" DESC");
                        break;
                }
            }
            else
            {
                commandText.Append("SELECT [pid],[psn],[cateid],[brandid],[storeid],[storecid],[storestid],[skugid],[name],[shopprice],[marketprice],[costprice],[state],[isbest],[ishot],[isnew],[displayorder],[weight],[showimg],[salecount],[visitcount],[reviewcount],[star1],[star2],[star3],[star4],[star5],[addtime] FROM");
                commandText.Append(" (SELECT ROW_NUMBER() OVER (ORDER BY ");
                switch (sortColumn)
                {
                    case 0:
                        commandText.Append("[temp].[relevancy]");
                        break;
                    case 1:
                        commandText.Append("[temp].[salecount]");
                        break;
                    case 2:
                        commandText.Append("[temp].[shopprice]");
                        break;
                    case 3:
                        commandText.Append("[temp].[reviewcount]");
                        break;
                    case 4:
                        commandText.Append("[temp].[addtime]");
                        break;
                    case 5:
                        commandText.Append("[temp].[visitcount]");
                        break;
                    default:
                        commandText.Append("[temp].[relevancy]");
                        break;
                }
                switch (sortDirection)
                {
                    case 0:
                        commandText.Append(" DESC");
                        break;
                    case 1:
                        commandText.Append(" ASC");
                        break;
                    default:
                        commandText.Append(" DESC");
                        break;
                }
                commandText.AppendFormat(") AS [rowid],[temp].[pid],[temp].[psn],[temp].[cateid],[temp].[brandid],[temp].[storeid],[temp].[storecid],[temp].[storestid],[temp].[skugid],[temp].[name],[temp].[shopprice],[temp].[marketprice],[temp].[costprice],[temp].[state],[temp].[isbest],[temp].[ishot],[temp].[isnew],[temp].[displayorder],[temp].[weight],[temp].[showimg],[temp].[salecount],[temp].[visitcount],[temp].[reviewcount],[temp].[star1],[temp].[star2],[temp].[star3],[temp].[star4],[temp].[star5],[temp].[addtime] from(SELECT distinct [p].[pid],[p].[psn],[p].[cateid],[p].[brandid],[p].[storeid],[p].[storecid],[p].[storestid],[p].[skugid],[p].[name],[p].[shopprice],[p].[marketprice],[p].[costprice],[p].[state],[p].[isbest],[p].[ishot],[p].[isnew],[p].[displayorder],[p].[weight],[p].[showimg],[p].[salecount],[p].[visitcount],[p].[reviewcount],[p].[star1],[p].[star2],[p].[star3],[p].[star4],[p].[star5],[p].[addtime],[pk].relevancy FROM [{0}products] AS [p]", RDBSHelper.RDBSTablePre);

                commandText.AppendFormat(" LEFT JOIN [{0}productkeywords] AS [pk] ON [p].[pid]=[pk].[pid]", RDBSHelper.RDBSTablePre);

                commandText.AppendFormat(" WHERE [p].[storeid]={0}", storeId);

                if (storeCid > 0)
                    commandText.AppendFormat(" AND [p].[storecid]={0}", storeCid);

                if (startPrice > 0)
                    commandText.AppendFormat(" AND [p].[shopprice]>={0}", startPrice);
                if (endPrice > 0)
                    commandText.AppendFormat(" AND [p].[shopprice]<={0}", endPrice);

                commandText.Append(" AND [p].[state]=0");

                if (!string.IsNullOrEmpty(keyword))
                {
                    commandText.AppendFormat(" AND( [p].[name] like '%{0}%' OR [pk].[keyword]='{0}')", keyword);
                }
                commandText.Append(") AS [temp]");
                commandText.Append(") AS [temp1]");
                commandText.AppendFormat(" WHERE [rowid] BETWEEN {0} AND {1}", pageSize * (pageNumber - 1) + 1, pageSize * pageNumber);
            }

            List<PartProductInfo> partProductList = new List<PartProductInfo>();
            IDataReader reader = RDBSHelper.ExecuteReader(CommandType.Text, commandText.ToString());
            while (reader.Read())
            {
                PartProductInfo partProductInfo = new PartProductInfo();

                partProductInfo.Pid = TypeHelper.ObjectToInt(reader["pid"]);
                partProductInfo.PSN = reader["psn"].ToString();
                partProductInfo.CateId = TypeHelper.ObjectToInt(reader["cateid"]);
                partProductInfo.BrandId = TypeHelper.ObjectToInt(reader["brandid"]);
                partProductInfo.StoreId = TypeHelper.ObjectToInt(reader["storeid"]);
                partProductInfo.StoreCid = TypeHelper.ObjectToInt(reader["storecid"]);
                partProductInfo.StoreSTid = TypeHelper.ObjectToInt(reader["storestid"]);
                partProductInfo.SKUGid = TypeHelper.ObjectToInt(reader["skugid"]);
                partProductInfo.Name = reader["name"].ToString();
                partProductInfo.ShopPrice = TypeHelper.ObjectToDecimal(reader["shopprice"]);
                partProductInfo.MarketPrice = TypeHelper.ObjectToDecimal(reader["marketprice"]);
                partProductInfo.CostPrice = TypeHelper.ObjectToDecimal(reader["costprice"]);
                partProductInfo.State = TypeHelper.ObjectToInt(reader["state"]);
                partProductInfo.IsBest = TypeHelper.ObjectToInt(reader["isbest"]);
                partProductInfo.IsHot = TypeHelper.ObjectToInt(reader["ishot"]);
                partProductInfo.IsNew = TypeHelper.ObjectToInt(reader["isnew"]);
                partProductInfo.DisplayOrder = TypeHelper.ObjectToInt(reader["displayorder"]);
                partProductInfo.Weight = TypeHelper.ObjectToInt(reader["weight"]);
                partProductInfo.ShowImg = reader["showimg"].ToString();
                partProductInfo.SaleCount = TypeHelper.ObjectToInt(reader["salecount"]);
                partProductInfo.VisitCount = TypeHelper.ObjectToInt(reader["visitcount"]);
                partProductInfo.ReviewCount = TypeHelper.ObjectToInt(reader["reviewcount"]);
                partProductInfo.Star1 = TypeHelper.ObjectToInt(reader["star1"]);
                partProductInfo.Star2 = TypeHelper.ObjectToInt(reader["star2"]);
                partProductInfo.Star3 = TypeHelper.ObjectToInt(reader["star3"]);
                partProductInfo.Star4 = TypeHelper.ObjectToInt(reader["star4"]);
                partProductInfo.Star5 = TypeHelper.ObjectToInt(reader["star5"]);
                partProductInfo.AddTime = TypeHelper.ObjectToDateTime(reader["addtime"]);

                partProductList.Add(partProductInfo);
            }
            reader.Close();

            return partProductList;
        }

        /// <summary>
        /// 获得搜索店铺商品数量
        /// </summary>
        /// <param name="keyword">关键词</param>
        /// <param name="storeId">店铺id</param>
        /// <param name="storeCid">店铺分类id</param>
        /// <param name="startPrice">开始价格</param>
        /// <param name="endPrice">结束价格</param>
        /// <returns></returns>
        public int GetSearchStoreProductCount(string keyword, int storeId, int storeCid, int startPrice, int endPrice)
        {
            StringBuilder commandText = new StringBuilder();

            commandText.AppendFormat("SELECT COUNT([temp].[pid]) FROM( SELECT DISTINCT [p].[pid] FROM [{0}products] AS [p]", RDBSHelper.RDBSTablePre);

            commandText.AppendFormat(" LEFT JOIN [{0}productkeywords] AS [pk] ON [p].[pid]=[pk].[pid]", RDBSHelper.RDBSTablePre);

            commandText.AppendFormat(" WHERE [p].[storeid]={0}", storeId);

            if (storeCid > 0)
                commandText.AppendFormat(" AND [p].[storecid]={0}", storeCid);

            if (startPrice > 0)
                commandText.AppendFormat(" AND [p].[shopprice]>={0}", startPrice);
            if (endPrice > 0)
                commandText.AppendFormat(" AND [p].[shopprice]<={0}", endPrice);

            commandText.Append(" AND [p].[state]=0");
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                commandText.AppendFormat(" AND([P].[name] like '%{0}%' OR [pk].[keyword]='{0}')", keyword);
            }
            commandText.Append(" )AS temp");
            return TypeHelper.ObjectToInt(RDBSHelper.ExecuteScalar(CommandType.Text, commandText.ToString()));
        }

        /// <summary>
        /// 获得分类列表
        /// </summary>
        /// <param name="keyword">关键词</param>
        /// <returns></returns>
        public List<CategoryInfo> GetCategoryListByKeyword(string keyword)
        {
            DbParameter[] parms = {
                                    GenerateInParam("@keyword", SqlDbType.NVarChar,255,keyword)
                                   };
            List<CategoryInfo> categoryList = new List<CategoryInfo>();
            IDataReader reader = RDBSHelper.ExecuteReader(CommandType.StoredProcedure,
                                                          string.Format("{0}getcategorylistbykeyword", RDBSHelper.RDBSTablePre),
                                                          parms);
            while (reader.Read())
            {
                CategoryInfo categoryInfo = new CategoryInfo();

                categoryInfo.CateId = TypeHelper.ObjectToInt(reader["cateid"]);
                categoryInfo.DisplayOrder = TypeHelper.ObjectToInt(reader["displayorder"]);
                categoryInfo.Name = reader["name"].ToString();
                categoryInfo.PriceRange = reader["pricerange"].ToString();
                categoryInfo.ParentId = TypeHelper.ObjectToInt(reader["parentid"]);
                categoryInfo.Layer = TypeHelper.ObjectToInt(reader["layer"]);
                categoryInfo.HasChild = TypeHelper.ObjectToInt(reader["haschild"]);
                categoryInfo.Path = reader["path"].ToString();

                categoryList.Add(categoryInfo);
            }
            reader.Close();
            return categoryList;
        }

        /// <summary>
        /// 获得分类品牌列表
        /// </summary>
        /// <param name="cateId">分类id</param>
        /// <param name="keyword">关键词</param>
        /// <returns></returns>
        public List<BrandInfo> GetCategoryBrandListByKeyword(int cateId, string keyword)
        {
            //找出该分类下的所有子分类id（包括自己）
            StringBuilder cateIds = GetAllCateIdsByParentCateId(cateId);
            string ids = cateIds.ToString().Trim();

            IDataReader reader =null;
            if (string.IsNullOrEmpty(keyword))
            {
                string commandText = string.Format(" SELECT [brandid],[displayorder] ,[name],[logo] FROM [bma_brands] WHERE [brandid] IN (SELECT DISTINCT [brandid] FROM [bma_products] WHERE [cateid] in ({1}) AND [state]=0)", RDBSHelper.RDBSTablePre, cateIds);
                reader = RDBSHelper.ExecuteReader(CommandType.Text, commandText);
            }
            
            else
            {
                DbParameter[] parms = {
                                    GenerateInParam("@cateids", SqlDbType.NChar,500,ids),
                                    GenerateInParam("@keyword", SqlDbType.NVarChar,255,keyword)
                                   };
                reader = RDBSHelper.ExecuteReader(CommandType.StoredProcedure,
                                                             string.Format("{0}getcategorybrandlistbykeyword", RDBSHelper.RDBSTablePre),
                                                             parms);
            }

            List<BrandInfo> brandList = new List<BrandInfo>();
            while (reader.Read())
            {
                BrandInfo brandInfo = new BrandInfo();

                brandInfo.BrandId = TypeHelper.ObjectToInt(reader["brandid"]);
                brandInfo.DisplayOrder = TypeHelper.ObjectToInt(reader["displayorder"]);
                brandInfo.Name = reader["name"].ToString();
                brandInfo.Logo = reader["logo"].ToString();

                brandList.Add(brandInfo);
            }
            reader.Close();
            return brandList;
        }


        #region 获取分类下所有的子分类id（包括自己）
        public StringBuilder GetAllCateIdsByParentCateId(int cateId)
        {
            StringBuilder cateIds = new StringBuilder();
            DbParameter[] parms =  { 
                                        GenerateInParam("@cateid", SqlDbType.Int, 4, cateId)
                                    };
            string commandSelectText = "";
            if (cateId > 0)
            {
                commandSelectText = string.Format("with [cte] as ( select [a].[cateid] from [{0}categories] a where [cateid]=@cateId union all select [k].[cateid] from [{0}categories] [k] inner join [cte] [c] on [c].[cateid] = [k].[parentid]  )select * from [cte] ", RDBSHelper.RDBSTablePre);
            }
            else
            {
                commandSelectText = string.Format("select [cateid] from [{0}categories]", RDBSHelper.RDBSTablePre);
            }

            DataTable table = RDBSHelper.ExecuteDataset(CommandType.Text, commandSelectText, parms).Tables[0];

            for (int i = 0; i < table.Rows.Count; i++)
            {
                cateIds.Append(table.Rows[i][0].ToString() + ",");
            }
            cateIds.Remove(cateIds.Length - 1, 1);
            return cateIds;
        }
        #endregion

        #region  辅助方法

        /// <summary>
        /// 生成输入参数
        /// </summary>
        /// <param name="paramName">参数名</param>
        /// <param name="sqlDbType">参数类型</param>
        /// <param name="size">类型大小</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        private DbParameter GenerateInParam(string paramName, SqlDbType sqlDbType, int size, object value)
        {
            SqlParameter param = new SqlParameter(paramName, sqlDbType, size);
            param.Direction = ParameterDirection.Input;
            if (value != null)
                param.Value = value;
            return param;
        }

        #endregion
    }
}
