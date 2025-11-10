using DriverLicenseTest.Application.Interfaces;
using DriverLicenseTest.Domain.Entities;
using DriverLicenseTest.Infrastructure.Data;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore.Storage;

namespace DriverLicenseTest.Application.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly DriverLicenseTestContext _context;
    private IDbContextTransaction? _transaction;

    private IGenericRepository<Category>? _categories;
    private IGenericRepository<Question>? _questions;
    private IGenericRepository<AnswerOption>? _answerOptions;
    private IGenericRepository<AspNetRole>? _roles;
    private IGenericRepository<AspNetUser>? _users;
    private IGenericRepository<LicenseType>? _licenseTypes;
    private IGenericRepository<LicenseQuestion>? _licenseQuestions;
    private IGenericRepository<UserPractice>? _userPractices;
    private IGenericRepository<MockExam>? _mockExams;
    private IGenericRepository<MockExamAnswer>? _mockExamAnswers;
    private IGenericRepository<UserWrongQuestion> _userWrongQuestions;
    private IGenericRepository<TrafficSign> _trafficSign;
    private IGenericRepository<UserStatistic> _userStatistic;
    private IGenericRepository<AspNetUserRole>? _userRoles;
    
    public UnitOfWork(DriverLicenseTestContext context)
    {
        _context = context;
    }
    public IGenericRepository<AspNetUserRole> UserRoles =>
        _userRoles ??= new GenericRepository<AspNetUserRole>(_context);

    public IGenericRepository<Category> Categories
        => _categories ??= new GenericRepository<Category>(_context);

    public IGenericRepository<Question> Questions
        => _questions ??= new GenericRepository<Question>(_context);

    public IGenericRepository<AnswerOption> AnswerOptions
        => _answerOptions ??= new GenericRepository<AnswerOption>(_context);

    public IGenericRepository<AspNetUser> Users
        => _users ??= new GenericRepository<AspNetUser>(_context);
    public IGenericRepository<AspNetRole> Roles
       => _roles ??= new GenericRepository<AspNetRole>(_context);

    public IGenericRepository<LicenseType> LicenseTypes
        => _licenseTypes ??= new GenericRepository<LicenseType>(_context);

    public IGenericRepository<LicenseQuestion> LicenseQuestions
        => _licenseQuestions ??= new GenericRepository<LicenseQuestion>(_context);

    public IGenericRepository<UserPractice> UserPractices
        => _userPractices ??= new GenericRepository<UserPractice>(_context);

    public IGenericRepository<MockExam> MockExams
        => _mockExams ??= new GenericRepository<MockExam>(_context);

    public IGenericRepository<MockExamAnswer> MockExamAnswers
        => _mockExamAnswers ??= new GenericRepository<MockExamAnswer>(_context);

    public IGenericRepository<UserWrongQuestion> UserWrongQuestions
    => _userWrongQuestions ??= new GenericRepository<UserWrongQuestion>(_context);
    public IGenericRepository<UserStatistic> UserStatistics
    => _userStatistic ??= new GenericRepository<UserStatistic>(_context);
    public IGenericRepository<TrafficSign> TrafficSigns
    => _trafficSign ??= new GenericRepository<TrafficSign>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}