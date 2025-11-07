using DriverLicenseTest.Domain.Entities;
using Infrastructure.Repository;
namespace DriverLicenseTest.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Category> Categories { get; }
    IGenericRepository<Question> Questions { get; }
    IGenericRepository<AnswerOption> AnswerOptions { get; }
    IGenericRepository<AspNetUser> Users { get; }
    IGenericRepository<LicenseType> LicenseTypes { get; }
    IGenericRepository<LicenseQuestion> LicenseQuestions { get; }
    IGenericRepository<UserPractice> UserPractices { get; }
    IGenericRepository<MockExam> MockExams { get; }
    IGenericRepository<MockExamAnswer> MockExamAnswers { get; }
    IGenericRepository<UserWrongQuestion> UserWrongQuestions { get; }
    IGenericRepository<TrafficSign> TrafficSigns { get; }
    IGenericRepository<UserStatistic> UserStatistics { get; }


    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
