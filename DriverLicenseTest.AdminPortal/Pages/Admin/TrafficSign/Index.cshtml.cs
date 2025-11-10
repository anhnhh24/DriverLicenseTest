// TrafficSigns/Index.cshtml.cs
using DriverLicenseTest.Application.Interfaces;
using DriverLicenseTest.Domain.Entities;
using DriverLicenseTest.Shared.DTOs.TrafficSign;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq.Expressions;
using static AdminApp.Pages.Admin.TrafficSigns.ExpressionExtensions;

namespace AdminApp.Pages.Admin.TrafficSigns
{
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public IndexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<TrafficSignDto> TrafficSigns { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SignType { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? IsActive { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 12;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        public async Task OnGetAsync()
        {
            Expression<Func<TrafficSign, bool>>? filter = null;

            if (!string.IsNullOrEmpty(SearchString))
            {
                filter = s => s.SignName.Contains(SearchString) || s.SignCode.Contains(SearchString);
            }

            if (!string.IsNullOrEmpty(SignType))
            {
                var typeFilter = new Func<TrafficSign, bool>(s => s.SignType == SignType);
                filter = filter == null ?
                    (Expression<Func<TrafficSign, bool>>)(s => s.SignType == SignType) :
                    filter.And(s => s.SignType == SignType);
            }

            if (IsActive.HasValue)
            {
                var activeFilter = new Func<TrafficSign, bool>(s => s.IsActive == IsActive.Value);
                filter = filter == null ?
                    (Expression<Func<TrafficSign, bool>>)(s => s.IsActive == IsActive.Value) :
                    filter.And(s => s.IsActive == IsActive.Value);
            }

            TotalCount = await _unitOfWork.TrafficSigns.GetCount(filter);
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            var signs = await _unitOfWork.TrafficSigns.GetListAsync(
                filter: filter,
                orderBy: q => q.OrderBy(s => s.SignCode),
                pageSize: PageSize,
                pageNumber: PageNumber
            );

            TrafficSigns = signs.Select(s => new TrafficSignDto
            {
                SignId = s.SignId,
                SignCode = s.SignCode,
                SignName = s.SignName,
                Description = s.Description,
                ImageURL = s.ImageUrl,
                SignType = s.SignType,
                Meaning = s.Meaning,
                RelatedQuestionCount = s.RelatedQuestionCount,
                IsActive = s.IsActive
            }).ToList();
        }

        public Dictionary<string, string> GetRouteData(int pageNumber)
        {
            var routeData = new Dictionary<string, string>
            {
                { "pageNumber", pageNumber.ToString() }
            };

            if (!string.IsNullOrEmpty(SearchString))
                routeData["searchString"] = SearchString;

            if (!string.IsNullOrEmpty(SignType))
                routeData["signType"] = SignType;

            if (IsActive.HasValue)
                routeData["isActive"] = IsActive.Value.ToString();

            return routeData;
        }
    }

    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T));

            var leftVisitor = new ReplaceParameterVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);

            var rightVisitor = new ReplaceParameterVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);

            var andAlso = Expression.AndAlso(left, right);
            return Expression.Lambda<Func<T, bool>>(andAlso, parameter);
        }

        private class ReplaceParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParameter;
            private readonly ParameterExpression _newParameter;

            public ReplaceParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _oldParameter ? _newParameter : base.VisitParameter(node);
            }
        }
    }
}
