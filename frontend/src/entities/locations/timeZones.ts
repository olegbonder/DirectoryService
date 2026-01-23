export function GetTimezones() {
  const timeZones = [
    { value: "Europe/Moscow", label: "Moscow (Europe/Moscow)" },
    { value: "Europe/London", label: "London (Europe/London)" },
    { value: "America/New_York", label: "New York (America/New_York)" },
    {
      value: "America/Los_Angeles",
      label: "Los Angeles (America/Los_Angeles)",
    },
    { value: "Asia/Tokyo", label: "Tokyo (Asia/Tokyo)" },
    { value: "Asia/Dubai", label: "Dubai (Asia/Dubai)" },
    { value: "Australia/Sydney", label: "Sydney (Australia/Sydney)" },
    { value: "Europe/Berlin", label: "Berlin (Europe/Berlin)" },
    { value: "Europe/Paris", label: "Paris (Europe/Paris)" },
    { value: "Europe/Madrid", label: "Madrid (Europe/Madrid)" },
    { value: "Asia/Shanghai", label: "Shanghai (Asia/Shanghai)" },
    { value: "Asia/Singapore", label: "Singapore (Asia/Singapore)" },
    { value: "Africa/Cairo", label: "Cairo (Africa/Cairo)" },
    {
      value: "Africa/Johannesburg",
      label: "Johannesburg (Africa/Johannesburg)",
    },
    { value: "America/Chicago", label: "Chicago (America/Chicago)" },
    { value: "America/Denver", label: "Denver (America/Denver)" },
    { value: "America/Vancouver", label: "Vancouver (America/Vancouver)" },
    { value: "Pacific/Auckland", label: "Auckland (Pacific/Auckland)" },
    { value: "Pacific/Honolulu", label: "Honolulu (Pacific/Honolulu)" },
  ];

  const timeZoneValues = timeZones.map((tz) => tz.value);
  return { timeZones, timeZoneValues };
}
