using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pohyb : MonoBehaviour {

	#region skok stuff
	public float _zrychlovacPadu, _silaSkoku, _silaLetu, _maxCasSkoku, _coyoteTime;
	private float _casSkoku;
	public int _skokuNavic;
	private int _skoku;
	[HideInInspector]
	public bool _naZemi = true;
	private bool _space, _spaceTuk, _spaceUp, _zeZeme;
	[SerializeField]
	private LayerMask _naCemMuzuHopsat;
	public Collider2D _nohy, _hlava;
	#endregion 

	#region pohyb stuff
	public float _rychlost, _stopak;
	private Vector2 _kam;
	public float _maxRychl, _zmenaSmeru;
	Rigidbody2D _rb;
	#endregion

	#region dash stuff
	public float _casDashe, _maxCasDashe, _rychlostDashe;
	private bool _dashuju, _muzuDash;
	private Vector2 _tempVel;

	public GameObject _rot;
	public BoxCollider2D _teloL, _teloR;
	#endregion

	#region cutscena
	[HideInInspector]
	public bool _cutscena;
	#endregion

	#region nasledovani
	public class Marker {

		public Vector3 _pozice;
		public float _xVel,_yVel;

		public Marker(Vector3 _poz, float _x, float _y){
			_pozice = _poz;
			_xVel = _x;
			_yVel = _y;
		}

	}

	public List<Marker> _markerList = new List<Marker>();
	public List<Marker> _markerList2 = new List<Marker>();

	public bool _aliz;
	public bool _curt;
	#endregion


	void Awake () {
		_rb = GetComponent<Rigidbody2D> ();
		_skoku = _skokuNavic;
		_zeZeme = false;
		_dashuju = false;
		_muzuDash = true;
		_markerList.Add (new Marker (transform.position, _rb.velocity.x,_rb.velocity.y));
		_markerList2.Add (new Marker (transform.position, _rb.velocity.x,_rb.velocity.y));
	}
	void Start(){
		_tempVel = _rb.velocity;
		_rb.velocity = Vector2.zero;
		_rb.gravityScale = 0;
		_dashuju = true;
		KonecDashe ();
	}

	void Update(){

		if (!_cutscena) {
			
			#region dash

			if (Input.GetButtonDown ("Dash") && _casDashe > 0 && _muzuDash) {
				_tempVel = _rb.velocity;
				_rb.velocity = Vector2.zero;
				_rb.gravityScale = 0;

				Vector2 _rozdil = Camera.main.ScreenToWorldPoint (Input.mousePosition) - _rot.transform.position;
				float _rotace = Mathf.Atan2 (_rozdil.y, _rozdil.x) * Mathf.Rad2Deg;
				_rot.transform.rotation = Quaternion.Euler (0, 0, _rotace);
				if (_naZemi && _rozdil.y < 0) {
					KonecDashe ();
				} else {
					_dashuju = true;
				}
			}
			if (_dashuju) {
				_casDashe -= Time.deltaTime;
			}
			if (_casDashe < 0) {
				KonecDashe ();
			}
			if (_dashuju) {
				if (_nohy.IsTouchingLayers (_naCemMuzuHopsat) && _rb.velocity.y < 0 || _hlava.IsTouchingLayers (_naCemMuzuHopsat) && _rb.velocity.y > 0 || _teloL.IsTouchingLayers (_naCemMuzuHopsat) && _rb.velocity.x < 0 || _teloR.IsTouchingLayers (_naCemMuzuHopsat) && _rb.velocity.x > 0) {
					KonecDashe ();
				}
			}

			#endregion

			//_naZemi = _nohy.IsTouchingLayers (_naCemMuzuHopsat);

			if (!_nohy.IsTouchingLayers (_naCemMuzuHopsat) && _naZemi) {
				StartCoroutine (CoyoteTime ());
			} else if (_nohy.IsTouchingLayers (_naCemMuzuHopsat) && !_naZemi) {
				_naZemi = true;
			}

			if (_naZemi) {
				_muzuDash = true;
			}

			if (Input.GetButton ("Jump") && _hlava.IsTouchingLayers (_naCemMuzuHopsat)) {
				_casSkoku = 0;
			}

			#region jump press
			_spaceUp = Input.GetButtonUp ("Jump");
			_space = Input.GetButton ("Jump");
			_spaceTuk = Input.GetButtonDown ("Jump");
			#endregion

			#region jump stuff
			if (_hlava.IsTouchingLayers (_naCemMuzuHopsat)) {
				_casSkoku = 0;
			}

			if (_naZemi) {
				_skoku = _skokuNavic;
				_casDashe = _maxCasDashe;
			}

			if (_spaceUp) {
				if (!_zeZeme) {
					_skoku--;
				}
				_zeZeme = false;
			}
			if (_spaceTuk) {
				_casSkoku = _maxCasSkoku;
				if (_naZemi) {
					_skoku = _skokuNavic;
					_zeZeme = true;
				}
			}
			if (_space) {
				if (_casSkoku > 0) {
					_casSkoku -= Time.deltaTime;
				}
			}
			#endregion

			_kam = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		}

		#region nasledovat stuff

		if(_rb.velocity != Vector2.zero){
			if(_aliz){
				UpdateMarkerList ();
			}
			if(_curt){
				UpdateMarkerList2 ();
			}
		}
		if(_markerList.Count == 0){
			_markerList.Add (new Marker (transform.position, _rb.velocity.x,_rb.velocity.y));
		}
		if(_markerList2.Count == 0){
			_markerList2.Add (new Marker (transform.position, _rb.velocity.x,_rb.velocity.y));
		}
		#endregion

	}

	void FixedUpdate () {
		if (_dashuju && _casDashe > 0) {
			_rb.velocity = _rot.transform.right * _rychlostDashe;
		}

		#region skok
		if(!_dashuju){
			if (_spaceTuk && _skoku > 0) {
				_rb.AddForce (Vector2.up * _silaSkoku, ForceMode2D.Impulse);
			}

			if (_space && _skoku > 0) {
				if (_casSkoku > 0) {
					_rb.velocity = new Vector2(_rb.velocity.x, _silaLetu);
				}
			}
			if (_casSkoku < 0 && !_naZemi || !_space && !_naZemi) {
				if (_rb.velocity.y > 0 && !_dashuju) {
					_rb.velocity += new Vector2(_rb.velocity.x, Physics2D.gravity.y * (_zrychlovacPadu - 1)) * Time.deltaTime;
				}
			}
		}
		#endregion

		#region horizontalni pohyb
		if(!_dashuju){
			_rb.AddForce(Vector2.right * _kam.x * _rychlost);

			if (_naZemi && Input.GetAxisRaw ("Horizontal") == 0) {
				_rb.velocity = new Vector2 ((Mathf.Lerp (_rb.velocity.x, 0f, _stopak * Time.deltaTime)), _rb.velocity.y);
			}
		
			if (Mathf.Abs (_rb.velocity.x) > _maxRychl) {
				_rb.velocity = new Vector2 (Mathf.Sign(_rb.velocity.x) *_maxRychl, _rb.velocity.y);
			}

			bool _b = (_kam.x > 0 && _rb.velocity.x < 0 || _kam.x < 0 && _rb.velocity.x > 0);

			if (_b && _naZemi || _kam.x == 0 &&_naZemi) {
				_rb.drag = _zmenaSmeru;
			} else if(_b){
				_rb.drag = _zmenaSmeru * 0.25f;
			} else {
				_rb.drag = 0;
			}
		}
		#endregion

	}

	public void UpdateMarkerList(){
		_markerList.Add (new Marker (transform.position, _rb.velocity.x,_rb.velocity.y));
	}
	public void VymazMarkerList(){
		_markerList.Clear ();
		_markerList.Add (new Marker (transform.position, _rb.velocity.x,_rb.velocity.y));
	}

	public void UpdateMarkerList2(){
		_markerList2.Add (new Marker (transform.position, _rb.velocity.x,_rb.velocity.y));
	}
	public void VymazMarkerList2(){
		_markerList2.Clear ();
		_markerList2.Add (new Marker (transform.position, _rb.velocity.x,_rb.velocity.y));
	}

	void KonecDashe(){
		_dashuju = false;
		_muzuDash = false;
		_rb.gravityScale = 1;
		_casDashe = _maxCasDashe;

		if (_tempVel.x > 0) {
			_rb.velocity = (_rot.transform.right * _tempVel.x + _rot.transform.up * 0.0001f * _tempVel.y);
		} else if (_tempVel.x < 0) {
			_rb.velocity = (-_rot.transform.right * _tempVel.x + _rot.transform.up * 0.0001f * _tempVel.y);
		} else {
			_rb.velocity = Vector3.zero;
		}
	}

	IEnumerator CoyoteTime(){
		yield return new WaitForSeconds (_coyoteTime);
		if (_naZemi) {
			_naZemi = false;
		}
	}

}
