using System;

using ColossalFramework;

using UnityEngine;

namespace CSLAPI.Utils
{
	public static class CitizenUtils
	{
		public static uint GetRandomCitizenId(Notification.Problem problems)
		{
			int index = SimulationManager.instance.m_randomizer.Int32(1, 0x7fff);
			for (int i = 0; i < 0x8000; i++)
			{
				if (++index == 0x8000)
				{
					index = 1;
				}
				if ((BuildingManager.instance.m_buildings.m_buffer[index].m_flags != Building.Flags.None) &&
				    ((BuildingManager.instance.m_buildings.m_buffer[index].m_problems & problems) != Notification.Problem.None))
				{
					uint citizenUnits = BuildingManager.instance.m_buildings.m_buffer[index].m_citizenUnits;
					uint randomCitizenId = GetRandomCitizenId(citizenUnits, CitizenUnit.Flags.Home | CitizenUnit.Flags.Work);
					if (randomCitizenId != 0)
					{
						return randomCitizenId;
					}
				}
			}
			return GetRandomResidentId();
		}

		public static uint GetRandomCitizenId(byte district)
		{
			int index = SimulationManager.instance.m_randomizer.Int32(1, 0x7fff);
			for (int i = 0; i < 0x8000; i++)
			{
				if (++index == 0x8000)
				{
					index = 1;
				}
				if (BuildingManager.instance.m_buildings.m_buffer[index].m_flags != Building.Flags.None)
				{
					Vector3 position = BuildingManager.instance.m_buildings.m_buffer[index].m_position;
					if (district == DistrictManager.instance.GetDistrict(position))
					{
						uint citizenUnits = BuildingManager.instance.m_buildings.m_buffer[index].m_citizenUnits;
						uint randomCitizenId = GetRandomCitizenId(citizenUnits, CitizenUnit.Flags.Home | CitizenUnit.Flags.Work);
						if (randomCitizenId != 0)
						{
							return randomCitizenId;
						}
					}
				}
			}
			return GetRandomResidentId();
		}

		public static uint GetRandomCitizenId(uint units, CitizenUnit.Flags flag)
		{
			CitizenManager instance = Singleton<CitizenManager>.instance;
			uint index = units;
			int num2 = 0;
			int num3 = 0;
			while (index != 0)
			{
				if (((ushort) (instance.m_units.m_buffer[index].m_flags & flag)) != 0)
				{
					for (int i = 0; i < 5; i++)
					{
						if (instance.m_units.m_buffer[index].GetCitizen(i) != 0)
						{
							num2++;
						}
					}
				}
				index = instance.m_units.m_buffer[index].m_nextUnit;
				if (++num3 > 0x80000)
				{
					CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
					break;
				}
			}
			if (num2 != 0)
			{
				num2 = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint) num2);
				index = units;
				num3 = 0;
				while (index != 0)
				{
					if (((ushort) (instance.m_units.m_buffer[index].m_flags & flag)) != 0)
					{
						for (int j = 0; j < 5; j++)
						{
							uint citizen = instance.m_units.m_buffer[index].GetCitizen(j);
							if ((citizen != 0) && (num2-- == 0))
							{
								return citizen;
							}
						}
					}
					index = instance.m_units.m_buffer[index].m_nextUnit;
					if (++num3 > 0x80000)
					{
						CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
						break;
					}
				}
			}
			return 0;
		}

		public static uint GetRandomResidentId()
		{
			int index = SimulationManager.instance.m_randomizer.Int32(1, 0x7fff);
			for (int i = 0; i < 0x8000; i++)
			{
				if (++index == 0x8000)
				{
					index = 1;
				}
				if ((BuildingManager.instance.m_buildings.m_buffer[index].m_flags != Building.Flags.None) &&
				    (BuildingManager.instance.m_buildings.m_buffer[index].Info.m_class.m_service == ItemClass.Service.Residential))
				{
					uint citizenUnits = BuildingManager.instance.m_buildings.m_buffer[index].m_citizenUnits;
					uint randomCitizenId = GetRandomCitizenId(citizenUnits, CitizenUnit.Flags.Home);
					if (randomCitizenId != 0)
					{
						return randomCitizenId;
					}
				}
			}
			return SimulationManager.instance.m_randomizer.UInt32(1, 0xfffff);
		}
	}
}