using System;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace WaterSystem
{
    public class BuoyancyBoat : MonoBehaviour
    {
        public float density; // density of the object, this is calculated off it's volume and mass
        public float volume; // volume of the object, this is calculated via it's colliders
        public float voxelResolution = 0.51f; // voxel resolution, represents the half size of a voxel when creating the voxel representation
        private Bounds _voxelBounds; // bounds of the voxels
        public Vector3 centerOfMass = Vector3.zero; // Center Of Mass offset
        public float waterLevelOffset = 0f;

        private int _guid; // GUID for the height system

        private Vector3[] _voxels; // voxel position
        private NativeArray<float3> _samplePoints; // sample points for height calc
        [NonSerialized] public float3[] Heights; // water height array(only size of 1 when simple or non-physical)
        private float3[] _normals; // water normal array(only used when non-physical and size of 1 also when simple)
        [SerializeField] Collider[] colliders; // colliders attatched ot this object
        private DebugDrawing[] _debugInfo; // For drawing force gizmos
        [NonSerialized] public float PercentSubmerged;

        [ContextMenu("Initialize")]
        private void Init()
        {
            _voxels = null;

            SetupVoxels();
            SetupData();
        }

        private void SetupVoxels()
        {
            _voxels = new Vector3[1];
            _voxels[0] = centerOfMass;
        }

        private void SetupData()
        {
            _debugInfo = new DebugDrawing[_voxels.Length];
            Heights = new float3[_voxels.Length];
            _normals = new float3[_voxels.Length];
            _samplePoints = new NativeArray<float3>(_voxels.Length, Allocator.Persistent);
        }

        private void OnEnable()
        {
            _guid = gameObject.GetInstanceID();
            Init();
            LocalToWorldConversion();
        }

        private void Update()
        {
#if STATIC_EVERYTHING
            var dt = 0.0f;
#else
            var dt = Time.deltaTime;
#endif
            var t = transform;
            var vec = t.position;
            vec.y = Heights[0].y + waterLevelOffset;
            t.position = vec;
            t.up = Vector3.Slerp(t.up, _normals[0], dt);

            GerstnerWavesJobs.UpdateSamplePoints(ref _samplePoints, _guid);
            GerstnerWavesJobs.GetData(_guid, ref Heights, ref _normals);
        }

        private void LateUpdate() { LocalToWorldConversion(); }

        private void OnDestroy()
        {
            CleanUp();
        }

        void CleanUp()
        {
            _samplePoints.Dispose();
        }

        private void LocalToWorldConversion()
        {
            var transformMatrix = transform.localToWorldMatrix;
            LocalToWorldJob.ScheduleJob(_guid, transformMatrix);
        }

        private Bounds VoxelBounds()
        {
            var bounds = new Bounds();
            foreach (var nextCollider in colliders)
            {
                bounds.Encapsulate(nextCollider.bounds);
            }
            return bounds;
        }

        private void OnDrawGizmosSelected()
        {
            const float gizmoSize = 0.05f;
            var t = transform;
            var matrix = Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);

            if (_voxels != null)
            {
                Gizmos.color = Color.yellow;

                foreach (var p in _voxels)
                {
                    Gizmos.DrawCube(p, new Vector3(gizmoSize, gizmoSize, gizmoSize));
                }
            }

            Gizmos.matrix = matrix;
            if (voxelResolution >= 0.1f)
            {
                Gizmos.DrawWireCube(_voxelBounds.center, _voxelBounds.size);
                Vector3 center = _voxelBounds.center;
                float y = center.y - _voxelBounds.extents.y;
                for (float x = -_voxelBounds.extents.x; x < _voxelBounds.extents.x; x += voxelResolution)
                {
                    Gizmos.DrawLine(new Vector3(x, y, -_voxelBounds.extents.z + center.z), new Vector3(x, y, _voxelBounds.extents.z + center.z));
                }
                for (float z = -_voxelBounds.extents.z; z < _voxelBounds.extents.z; z += voxelResolution)
                {
                    Gizmos.DrawLine(new Vector3(-_voxelBounds.extents.x, y, z + center.z), new Vector3(_voxelBounds.extents.x, y, z + center.z));
                }
            }
            else
                _voxelBounds = VoxelBounds();

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_voxelBounds.center + centerOfMass, 0.2f);

            Gizmos.matrix = Matrix4x4.identity; Gizmos.matrix = Matrix4x4.identity;

            if (_debugInfo != null)
            {
                foreach (DebugDrawing debug in _debugInfo)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawCube(debug.Position, new Vector3(gizmoSize, gizmoSize, gizmoSize)); // drawCenter
                    var water = debug.Position;
                    water.y = debug.WaterHeight;
                    Gizmos.DrawLine(debug.Position, water); // draw the water line
                    Gizmos.DrawSphere(water, gizmoSize * 4f);
                }
            }
        }

        private struct DebugDrawing
        {
            public Vector3 Force;
            public Vector3 Position;
            public float WaterHeight;
        }
    }
}