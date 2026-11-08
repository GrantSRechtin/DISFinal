using System.Collections.Generic;
using UnityEngine;

public class RewindableBase : MonoBehaviour, IRewindable
{

    public bool IsTimeStopped = false;

    protected struct Timestep
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 LinearVelocity;
        public Vector3 AngularVelocity;
        public HashSet<int> LinkNums;
        public Dictionary<int, IGravitizable> Partners;
        public Dictionary<int, GravityMode> Modes;
        public Dictionary<int, Vector3> Anchors;
        public bool IsSelected;
        public int CurrentLinkNum;
        public List<IGravitizable>[] GravLinks;
        public GravityMode[] GravModes;
        public IGravitizable LineA;
        public IGravitizable LineB;
        public bool LineVisible;
    }

    RewindController rewindController;
    
    protected virtual void OnEnable()
    {
        rewindController = RewindController.Instance;
        rewindController.TimeStarted    += OnTimeStarted;
        rewindController.TimeStopped    += OnTimeStopped;
        rewindController.RewindTimestep += OnRewindTimestep;
        rewindController.StoreTimestep  += OnStoreTimestep;
    }

    protected virtual void OnDisable()
    {   
        if (rewindController != null)
        {
            rewindController.TimeStarted    -= OnTimeStarted;
            rewindController.TimeStopped    -= OnTimeStopped;
            rewindController.RewindTimestep -= OnRewindTimestep;
            rewindController.StoreTimestep  -= OnStoreTimestep;
            rewindController = null;
        }
    }

    public virtual void OnTimeStarted() => IsTimeStopped = false;

    public virtual void OnTimeStopped() => IsTimeStopped = true;

    public virtual void OnStoreTimestep(){}

    public virtual void OnRewindTimestep(){}
}