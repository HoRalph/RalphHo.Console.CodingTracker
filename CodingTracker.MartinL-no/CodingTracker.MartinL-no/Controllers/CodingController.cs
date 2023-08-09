﻿using CodingTracker.MartinL_no.DAL;
using CodingTracker.MartinL_no.Models;

namespace CodingTracker.MartinL_no.Controllers;

internal class CodingController
{
    private readonly ICodingSessionRepository _sessionRepository;
    private readonly CodingGoalRepository _goalsRepository;

    public CodingController(ICodingSessionRepository sessionRepository, CodingGoalRepository goalsRepository)
	{
        _sessionRepository = sessionRepository;
        _goalsRepository = goalsRepository;
    }

    public DateTime StartSession()
    {
        return DateTime.Now;
    }

    public List<CodingSession> GetCodingSessions()
    {
        return _sessionRepository.GetCodingSessions();
    }

    public CodingSession GetCodingSession(int id)
    {
        return _sessionRepository.GetCodingSession(id);
    }

    private List<CodingSession> GetCodingSessionsFromDate(DateTime fromDateTime)
    {
        return _sessionRepository.GetCodingSessionFromDate(fromDateTime);
    }

    public List<CodingSession> GetCodingSessionsByDays(int days)
    {
        var period = new TimeSpan(days, 0, 0, 0);
        var fromDateTime = DateTime.Now.Subtract(period);

        return GetCodingSessionsFromDate(fromDateTime);
    }

    public List<CodingSession> GetCodingSessionsByMonths(int months)
    {
        var fromDateTime = DateTime.Now.AddMonths(months * -1);

        return GetCodingSessionsFromDate(fromDateTime);
    }

    public List<CodingSession> GetCodingSessionsByYears(int years)
    {
        var fromDateTime = DateTime.Now.AddYears(years * -1);

        return GetCodingSessionsFromDate(fromDateTime);
    }

    public bool InsertCodingSession(string startTimeString, string endTimeString)
    {
        var startTime = DateTime.Parse(startTimeString);
        var endTime = DateTime.Parse(endTimeString);
        var codingSession = new CodingSession(startTime, endTime);

        return _sessionRepository.InsertCodingSession(codingSession);
    }

    public bool DeleteCodingSession(int id)
    {
        return _sessionRepository.DeleteCodingSession(id);
    }

    public bool UpdateCodingSession(int id, string startTimeString, string endTimeString)
    {
        var codingSession = GetCodingSession(id);
        if (codingSession == null) return false;

        codingSession.StartTime = DateTime.Parse(startTimeString);
        codingSession.EndTime = DateTime.Parse(endTimeString);

        return _sessionRepository.UpdateCodingSession(codingSession);
    }

    public List<PeriodStatistic> GetStatisicsByDay()
    {
        var sessionsGroupedByDay = GetCodingSessions().GroupBy(s => s.StartTime.Date);

        return CalculateStatistics(Period.Day, sessionsGroupedByDay);
    }

    public List<PeriodStatistic> GetStatisicsByWeek()
    {
        var sessionsGroupedByWeek = GetCodingSessions()
            .GroupBy(s =>
            {
                var weekNumber = System.Globalization.ISOWeek.GetWeekOfYear(s.StartTime);
                return System.Globalization.ISOWeek.ToDateTime(s.StartTime.Year, weekNumber, DayOfWeek.Monday);
            });

        return CalculateStatistics(Period.Week, sessionsGroupedByWeek);
    }

    public List<PeriodStatistic> GetStatisicsByMonth()
    {
        var sessionsGroupedByMonth = GetCodingSessions().GroupBy(s => new DateTime(s.StartTime.Year, s.StartTime.Month, 1));

        return CalculateStatistics(Period.Month, sessionsGroupedByMonth);
    }

    private List<PeriodStatistic> CalculateStatistics(Period periodEnum, IEnumerable<IGrouping<DateTime,CodingSession>> periods)
    {
        var statistics = new List<PeriodStatistic>();

        foreach (var period in periods)
        {
            var date = new DateOnly(period.Key.Year, period.Key.Month, period.Key.Day);
            var sessionDurations = period.Select(session => session.EndTime - session.StartTime);
            var total = new TimeSpan(sessionDurations.Sum(timeSpan => timeSpan.Ticks));
            var average = new TimeSpan(Convert.ToInt64(sessionDurations.Average(t => t.Ticks)));

            statistics.Add(new PeriodStatistic(periodEnum, date, total, average));
        }

        return statistics;
    }

    public List<CodingGoal> GetCodingGoals()
    {
        var goals = _goalsRepository.GetCodingGoals();
        var sessions = _sessionRepository.GetCodingSessions();

        foreach (var goal in goals)
        {
            goal.TimeCompleted = CalculateTimeCompleted(goal, sessions);
            goal.HoursPerDayToComplete = CalculateHoursPerDayToComplete(goal, sessions, goal.TimeCompleted);
        }

        return goals;
    }

    private TimeSpan CalculateTimeCompleted(CodingGoal goal, List<CodingSession> sessions)
    {
        var sessionsInPeriod = sessions.Where(s => s.StartTime > goal.StartTime && s.StartTime < goal.EndTime);
        var sessionDurations = sessionsInPeriod.Select(session => session.EndTime - session.StartTime);
        var timeCompleted = new TimeSpan(sessionDurations.Sum(timeSpan => timeSpan.Ticks));

        return timeCompleted;
    }

    private TimeSpan CalculateHoursPerDayToComplete(CodingGoal goal, List<CodingSession> sessions, TimeSpan timeCompleted)
    {
        var goalHours = new TimeSpan(goal.Hours,0,0);
        var goalTimeRemaining = goalHours - goal.TimeCompleted;

        var timeToDeadline = goal.EndTime - DateTime.Now;
        var hoursPerDayToComplete = goalTimeRemaining / timeToDeadline.Days;

        return hoursPerDayToComplete;
    }

    public bool InsertCodingGoal(string endTimeString, int hours)
    {
        var startTime = DateTime.Now;
        var endTime = DateTime.Parse(endTimeString);
        var codingGoal = new CodingGoal(startTime, endTime, hours);

        return _goalsRepository.InsertCodingGoal(codingGoal);
    }
}
