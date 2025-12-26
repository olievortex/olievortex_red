namespace OlievortexRed.Lib;

public static class OlieStates
{
    private const string Data = """
                                    Alabama, 1, AL
                                    Alaska, 2, AK
                                    Arizona, 4, AZ
                                    Arkansas, 5, AR    
                                    California, 6, CA
                                    Colorado, 8, CO
                                    Connecticut, 9, CT
                                    Delaware, 10, DE
                                    District Of Columbia, 11, DC
                                    Florida, 12, FL
                                    Georgia, 13, GA
                                    Hawaii, 15, HI
                                    Idaho, 16, ID
                                    Illinois, 17, IL
                                    Indiana, 18, IN
                                    Iowa, 19, IA
                                    Kansas, 20, KS
                                    Kentucky, 21, KY
                                    Louisiana, 22, LA
                                    Maine, 23, ME
                                    Maryland, 24, MD
                                    Massachusetts, 25, MA
                                    Michigan, 26, MI
                                    Minnesota, 27, MN
                                    Mississippi, 28, MS
                                    Missouri, 29, MO
                                    Montana, 30, MT
                                    Nebraska, 31, NE
                                    Nevada, 32, NV
                                    New Hampshire, 33, NH
                                    New Jersey, 34, NJ
                                    New Mexico, 35, NM
                                    New York, 36, NY
                                    North Carolina, 37, NC
                                    North Dakota, 38, ND
                                    Ohio, 39, OH
                                    Oklahoma, 40, OK
                                    Oregon, 41, OR
                                    Pennsylvania, 42, PA
                                    Rhode Island, 44, RI
                                    South Carolina, 45, SC
                                    South Dakota, 46, SD
                                    Tennessee, 47, TN
                                    Texas, 48, TX
                                    Utah, 49, UT
                                    Vermont, 50, VT
                                    Virginia, 51, VA
                                    Washington, 53, WA
                                    West Virginia, 54, WV
                                    Wisconsin, 55, WI
                                    Wyoming, 56, WY
                                    Lake St Clair, 81, LakeStClair
                                    Hawaii Waters, 84, HawaiiWaters
                                    Gulf Of America, 85, GulfOfAmerica
                                    East Pacific, 86, EastPacific
                                    Atlantic South, 87, AtlanticSouth
                                    Atlantic North, 88, AtlanticNorth
                                    Gulf Of Alaska, 89, GulfOfAlaska
                                    Lake Huron, 90, LakeHuron
                                    Lake Michigan, 91, LakeMichigan
                                    Lake Superior, 92, LakeSuperior
                                    St Lawrence, 93, StLawrence
                                    Lake Ontario, 94, LakeOntario
                                    Lake Erie, 95, LakeErie
                                    Virgin Islands, 96, VI
                                    American Samoa, 97, AS
                                    Guam, 98, GU
                                    Puerto Rico, 99, PR
                                """;

    public static Dictionary<string, string> FullToAbbr { get; } = [];
    public static Dictionary<string, string> AbbrToFull { get; } = [];

    static OlieStates()
    {
        var lines = Data
            .ReplaceLineEndings("\n")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        foreach (var line in lines)
        {
            var parts = line.Split(',', StringSplitOptions.TrimEntries);

            FullToAbbr.Add(parts[0], parts[2]);
            AbbrToFull.Add(parts[2], parts[0]);
        }
    }
}