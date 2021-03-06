﻿// <copyright file="RealTimeResidentAI.Home.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using RealTime.Tools;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private void DoScheduledHome(ref CitizenSchedule schedule, TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort homeBuilding = CitizenProxy.GetHomeBuilding(ref citizen);
            if (homeBuilding == 0)
            {
                Log.Debug($"WARNING: {GetCitizenDesc(citizenId, ref citizen)} is in corrupt state: want to go home with no home building. Releasing the poor citizen.");
                CitizenMgr.ReleaseCitizen(citizenId);
                schedule = default;
                return;
            }

            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
            CitizenProxy.RemoveFlags(ref citizen, Citizen.Flags.Evacuating);
            CitizenProxy.SetVisitPlace(ref citizen, citizenId, 0);
            residentAI.StartMoving(instance, citizenId, ref citizen, currentBuilding, homeBuilding);
            schedule.Schedule(ResidentState.Unknown, default);
            Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is going from {currentBuilding} back home");
        }

        private bool RescheduleAtHome(ref CitizenSchedule schedule, ref TCitizen citizen)
        {
            if (schedule.CurrentState != ResidentState.AtHome || TimeInfo.Now < schedule.ScheduledStateTime)
            {
                return false;
            }

            if (schedule.ScheduledState != ResidentState.Relaxing && schedule.ScheduledState != ResidentState.Shopping)
            {
                return false;
            }

            if (IsBadWeather())
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(0, ref citizen)} re-schedules an activity because of bad weather (see next line for citizen ID)");
                schedule.Schedule(ResidentState.Unknown, default);
                return true;
            }

            uint goOutChance = spareTimeBehavior.GetGoOutChance(
                CitizenProxy.GetAge(ref citizen),
                schedule.WorkShift,
                schedule.ScheduledState == ResidentState.Shopping);

            if (Random.ShouldOccur(goOutChance))
            {
                return false;
            }

            Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(0, ref citizen)} re-schedules an activity because of time (see next line for citizen ID)");
            schedule.Schedule(ResidentState.Unknown, default);
            return true;
        }
    }
}
