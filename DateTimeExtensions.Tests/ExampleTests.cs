﻿using System;
using NUnit.Framework;
using System.Globalization;
using System.Threading;
using DateTimeExtensions;
using DateTimeExtensions.Strategies;

namespace DateTimeExtensions.Tests {
	[TestFixture]
	public class ExampleTests {

		[Test]
		public void simple_calculation() {
			var friday = new DateTime(2011,5,13); // A friday
			var friday_plus_two_working_days = friday.AddWorkingDays(2); // friday + 2 working days
			
			Assert.IsTrue(friday_plus_two_working_days == friday.AddDays(4));
			Assert.IsTrue(friday_plus_two_working_days.DayOfWeek == DayOfWeek.Tuesday);

			//not recomended because the default WorkingDayCultureInfo by default is pulled from current CultureInfo
		}

		[Test]
		public void recomended_calculation() {
			var workingDayCultureInfo = new WorkingDayCultureInfo("pt-PT");
			var friday = new DateTime(2011, 5, 13); // A friday
			var friday_plus_two_working_days = friday.AddWorkingDays(2, workingDayCultureInfo); // friday + 2 working days

			Assert.IsTrue(friday_plus_two_working_days == friday.AddDays(4));
			Assert.IsTrue(friday_plus_two_working_days.DayOfWeek == DayOfWeek.Tuesday);
		}

		[Test]
		public void globally_recomended_calculation() {
			//Ensure we're running on portuguese context
			Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-PT");

			var friday = new DateTime(2011, 5, 13); // A friday
			var friday_plus_two_working_days = friday.AddWorkingDays(2); // friday + 2 working days

			Assert.IsTrue(friday_plus_two_working_days == friday.AddDays(4));
			Assert.IsTrue(friday_plus_two_working_days.DayOfWeek == DayOfWeek.Tuesday);
		}

		[Test]
		public void holidays() {
			var ptWorkingDayCultureInfo = new WorkingDayCultureInfo("pt-PT");
			var enWorkingDayCultureInfo = new WorkingDayCultureInfo("en-US");

			var thursday = new DateTime(2011, 4, 21); // A thursday
			var thursday_plus_two_working_days_pt = thursday.AddWorkingDays(2, ptWorkingDayCultureInfo); // friday + 2 working days on PT
			var thursday_plus_two_working_days_en = thursday.AddWorkingDays(2, enWorkingDayCultureInfo); // friday + 2 working days on PT

			//English doesn't have supported holidays
			Assert.IsTrue(thursday_plus_two_working_days_en == thursday.AddDays(4));
			Assert.IsTrue(thursday_plus_two_working_days_en.DayOfWeek == DayOfWeek.Monday);

			//Portuguese Holidays are supported
			Assert.IsTrue(thursday_plus_two_working_days_pt == thursday.AddDays(6)); // + Good Friday (22-4-2011) + Carnation Revolution (25-4-2011)
			Assert.IsTrue(thursday_plus_two_working_days_pt.DayOfWeek == DayOfWeek.Wednesday);
		}

		[Test]
		public void check_working_day() {
			var ptWorkingDayCultureInfo = new WorkingDayCultureInfo("pt-PT");
			var carnationRevolution = new DateTime(2011, 4, 25);
			var nextDay = carnationRevolution.AddDays(1); 

			Assert.IsTrue(carnationRevolution.IsWorkingDay() == false);
			Assert.IsTrue(carnationRevolution.DayOfWeek == DayOfWeek.Monday);

			Assert.IsTrue(nextDay.IsWorkingDay() == true);
			Assert.IsTrue(nextDay.DayOfWeek == DayOfWeek.Tuesday);
		}

		/* Extensibility */

		public class CustomHolidayStrategy : IHolidayStrategy {
			public bool IsHoliDay(DateTime day) {
				if (day.Date == DateTime.Today)
					return true;
				return false;
			}
		}

		public class CustomeWorkingDayOfWeekStrategy : IWorkingDayOfWeekStrategy {
			public bool IsWorkingDay(DayOfWeek dayOfWeek) {
				return true;
			}
		}

		[Test]
		public void provide_custom_strategies() {
			var customWorkingDayCultureInfo = new WorkingDayCultureInfo() {
				LocateHolidayStrategy = (name) => new CustomHolidayStrategy() ,
				LocateWorkingDayOfWeekStrategy = (name) => new CustomeWorkingDayOfWeekStrategy()
			};

			Assert.IsTrue(DateTime.Today.IsWorkingDay(customWorkingDayCultureInfo) == false);
			Assert.IsTrue(DateTime.Today.AddDays(1).IsWorkingDay(customWorkingDayCultureInfo) == true);
		}

		public class CustomWorkingDayCultureInfo : IWorkingDayCultureInfo {

			public bool IsWorkingDay(DateTime date) {
				if (date.Date == DateTime.Today)
					return false;
				return true;
			}

			public bool IsWorkingDay(DayOfWeek dayOfWeek) {
				switch (dayOfWeek) {
					case DayOfWeek.Sunday:
					case DayOfWeek.Saturday:
					case DayOfWeek.Friday:
						return false;
					default:
						return true;
				}
			}

			public string Name {
				get { return "Hello World!"; }
			}
		}

		[Test]
		public void provide_custom_culture() {
			var customWorkingDayCultureInfo = new CustomWorkingDayCultureInfo();
			var today = DateTime.Today;
			var next_friday = new DateTime(2011, 05, 20);

			Assert.IsTrue(today.IsWorkingDay(customWorkingDayCultureInfo) == false);
			Assert.IsTrue(next_friday.IsWorkingDay(customWorkingDayCultureInfo) == false);
		}
	}
}