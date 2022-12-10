using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;

namespace Aurora.EffectsEngine.Animations;

public sealed class AnimationMix: IEquatable<AnimationMix>
{
    [JsonProperty]
    private readonly ConcurrentDictionary<string, AnimationTrack> _tracks = new();

    /// <summary>
    /// When true, will remove Animation tracks that no longer have any animations.
    /// </summary>
    [JsonProperty("_automatically_remove_complete")]
    private bool _automaticallyRemoveComplete;

    public AnimationMix()
    {
    }

    public AnimationMix(AnimationTrack[] tracks)
    {
        foreach (var track in tracks)
            AddTrack(track);
    }

    public AnimationMix SetAutoRemove(bool value)
    {
        _automaticallyRemoveComplete = value;

        return this;
    }

    public AnimationMix AddTrack(AnimationTrack track)
    {
        if (track != null)
        {
            if (_tracks.ContainsKey(track.GetName()))
                _tracks[track.GetName()] = track;
            else
                _tracks.TryAdd(track.GetName(), track);
        }

        return this;
    }

    private void RemoveTrack(string trackName)
    {
        _tracks.TryRemove(trackName, out _);
    }

    public bool ContainsTrack(string trackName)
    {
        return _tracks.ContainsKey(trackName);
    }

    public float GetDuration()
    {
        return _tracks.Select(track => track.Value.GetShift() + track.Value.AnimationDuration)
            .Prepend(0.0f)
            .Max();
    }

    public void SetScale(float scale)
    {
        foreach (KeyValuePair<string, AnimationTrack> track in _tracks)
        {
            track.Value.SetScale(scale);
        }
    }

    public ConcurrentDictionary<string, AnimationTrack> GetTracks()
    {
        return _tracks;
    }

    public bool AnyActiveTracksAt(float time)
    {
        return _tracks.Any(track => track.Value.ContainsAnimationAt(time));
    }

    public void Draw(Graphics g, float time, PointF offset = default)
    {
        foreach (KeyValuePair<string, AnimationTrack> track in _tracks)
        {
            if (track.Value.ContainsAnimationAt(time))
            {
                AnimationFrame frame = track.Value.GetFrame(time);
                frame.SetOffset(offset);
                frame.Draw(g);
            }
            else if (_automaticallyRemoveComplete)
            {
                RemoveTrack(track.Key);
            }
        }
    }

    public void Clear()
    {
        _tracks.Clear();
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((AnimationMix) obj);
    }

    public bool Equals(AnimationMix p)
    {
        return _tracks.Equals(p._tracks) &&
               _automaticallyRemoveComplete == p._automaticallyRemoveComplete;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + _tracks.GetHashCode();
            return hash;
        }
    }

    public override string ToString()
    {
        return "AnimationMix: [ Count: " + _tracks.Count + " ]";
    }
}