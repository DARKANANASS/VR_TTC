
public static class EventHelper
{
    public static void AddIsi(Results res, float duration)
    {
        res.Events.Add(new TimeEvent("isi_start", duration + 900));
        res.Events.Add(new TimeEvent("isi_end", duration + 1800));
    }

    public static void AddMove(Results res, float duration)
    {
        res.Events.Add(new TimeEvent("move_start", duration + 900));
        res.Events.Add(new TimeEvent("move_end", duration + 1800));
    }

    public static void AddStim(Results res, float duration)
    {
        res.Events.Add(new TimeEvent("stim_start", 900));
        res.Events.Add(new TimeEvent("stim_end", duration + 900));
    }

    public static void AddOcc(Results res, float duration)
    {
        res.Events.Add(new TimeEvent("occluder_moving", duration));
        res.Events.Add(new TimeEvent("ref_move_to_target", duration + 500));
    }

}
