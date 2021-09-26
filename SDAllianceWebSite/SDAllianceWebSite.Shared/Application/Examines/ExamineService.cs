using SDAllianceWebSite.Shared.Application.Dtos;
using SDAllianceWebSite.Shared.Application.Examines.Dtos;
using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using System.Collections.Generic;
using System.Linq;

namespace SDAllianceWebSite.Shared.Application.Examines
{
    public class ExamineService : IExamineService
    {
        public enum GetExaminePagedType
        {
            Entry,
            User
        }

        public PagedResultDto<ExaminedNormalListModel> GetPaginatedResult(GetExamineInput input, List<ExaminedNormalListModel> examines, GetExaminePagedType type)
        {
            IEnumerable<ExaminedNormalListModel> query = examines;
            if (type == GetExaminePagedType.Entry)
            {
                query = examines.Where(s => s.IsPassed == true);
            }
            //判断是否是条件筛选
            if (!string.IsNullOrWhiteSpace(input.ScreeningConditions))
            {
                switch (input.ScreeningConditions)
                {
                    case "待审核":
                        query = query.Where(s => s.IsPassed == null);
                        break;
                    case "已通过":
                        query = query.Where(s => s.IsPassed == true);
                        break;
                    case "未通过":
                        query = query.Where(s => s.IsPassed == false);
                        break;

                }
            }
            //判断输入的查询名称是否为空
            if (!string.IsNullOrWhiteSpace(input.FilterText))
            {
                //尝试将查询翻译成操作
                Operation operation = Operation.None;
                switch (input.FilterText)
                {
                    case "修改用户主页":
                        operation = Operation.UserMainPage;
                        break;
                    case "编辑文章主要信息":
                        operation = Operation.EditArticleMain;
                        break;
                    case "编辑文章关联词条":
                        operation = Operation.EditArticleRelevanes;
                        break;
                    case "编辑文章内容":
                        operation = Operation.EditArticleMainPage;
                        break;
                }
                query = query.Where(s => s.UserName.Contains(input.FilterText)
                  || s.Operation == operation);
            }
            //统计查询数据的总条数
            var count = examines.Count;
            //根据需求进行排序，然后进行分页逻辑的计算
            query = query.Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(input.MaxResultCount);

            //将结果转换为List集合 加载到内存中
            List<ExaminedNormalListModel> models = null;
            if (count != 0)
            {
                models = query.ToList();
            }
            else
            {
                models = new List<ExaminedNormalListModel>();
            }


            var dtos = new PagedResultDto<ExaminedNormalListModel>
            {
                TotalCount = count,
                CurrentPage = input.CurrentPage,
                MaxResultCount = input.MaxResultCount,
                Data = models,
                FilterText = input.FilterText,
                Sorting = input.Sorting,
                ScreeningConditions = input.ScreeningConditions
            };

            return dtos;
        }
    }
}
