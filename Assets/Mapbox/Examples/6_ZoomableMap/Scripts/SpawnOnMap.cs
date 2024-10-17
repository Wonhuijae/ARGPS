namespace Mapbox.Examples
{
	using UnityEngine;
	using Mapbox.Utils;
	using Mapbox.Unity.Map;
	using Mapbox.Unity.MeshGeneration.Factories;
	using Mapbox.Unity.Utilities;
	using System.Collections.Generic;
    using global::Unity.VisualScripting;

	public class SpawnOnMap : MonoBehaviour
	{
		[SerializeField]
        protected AbstractMap _map;

		[SerializeField]
		[Geocode]
        protected List<string> _locationStrings;
        protected List<Vector2d> _locations;

		[SerializeField]
        protected float _spawnScale = 50f;

		[SerializeField]
		protected GameObject _markerPrefab;

        protected List<GameObject> _spawnedObjects;

		protected virtual void Start()
		{
            _locationStrings = new List<string>();
            _locations = new List<Vector2d>();
			_spawnedObjects = new List<GameObject>();

			for (int i = 0; i < _locationStrings.Count; i++)
			{
				var locationString = _locationStrings[i];
				_locations[i] = Conversions.StringToLatLon(locationString);
				var instance = Instantiate(_markerPrefab);
                instance.transform.localPosition = _map.GeoToWorldPosition(_locations[i], true);
                instance.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
                _spawnedObjects.Add(instance);
            }
		}

		private void Update()
		{
			int count = _spawnedObjects.Count;
			for (int i = 0; i < count; i++)
			{
				var spawnedObject = _spawnedObjects[i];
				var location = _locations[i];
				spawnedObject.transform.localPosition = _map.GeoToWorldPosition(location, true);
				spawnedObject.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
			}
        }

        public void AddMarker(GameObject _instance, string _locString, Vector2d _loc)
        {
            _instance.transform.localPosition = _map.GeoToWorldPosition(_loc, true);
            _instance.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
			
            _spawnedObjects.Add(_instance);
			_locationStrings.Add(_locString);
			_locations.Add(_loc);
        }
    }
}