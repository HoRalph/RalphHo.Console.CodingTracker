using System;
using System.Configuration;
using System.Collections.Specialized;
using Microsoft.Data.Sqlite;
using System.Dynamic;
using System.Globalization;
using System.Runtime.CompilerServices;

public class CodingSession
{
    //startTime, endTime, Duration
    public  string startDateString;
    public  string endDateString;
    private DateTime startDate;
    private DateTime endDate;
    private TimeSpan duration;

    public string StartDateString
    {
        get { return startDateString; }
        set { startDateString = value;}
    }
    public string EndDateString
    {
        get { return endDateString; }
        set { endDateString = value;}
    }
    public DateTime StartDate
    {
        get { return startDate; }
        set { startDate = ConvertDate(this.startDateString);}
    }
    public DateTime EndTime
    {
        get {return endDate;}
        set {endDate = ConvertDate(this.endDateString);}
    }
    public TimeSpan Duration
    {
        get {return duration;}
        set {duration = this.endDate - this.startDate;}
    }
    public DateTime ConvertDate(string DateString)
    {
        DateTime dateValue = DateTime.Today;
        CultureInfo enUS = new CultureInfo("en-US");
        if(Validation.ValidDateTimeFormat(DateString))
        {
            DateTime.TryParseExact(DateString,"MM/dd/yyyy HH:MM",enUS, DateTimeStyles.None, out dateValue);
        }
        return dateValue;
    }
}