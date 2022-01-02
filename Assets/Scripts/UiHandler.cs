using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UiHandler : MonoBehaviour {
    public static int tao = 35;
    [SerializeField] private Text taoState;
    [SerializeField] private GameObject filterCreator;
    [SerializeField] private Slider slider;
    private FilterGenerator filterGenerator;
    public void Start() {
        filterGenerator = filterCreator.GetComponent<FilterGenerator>();
    }

    public void taoChange(float t) {
        tao = (int)t;
        taoState.text = tao.ToString();
    }

    public void timeFilterMove(float d) { filterGenerator.timeFilterMoved(d); }
    public void filterMove(float d) { filterGenerator.filterMoved((int)d); }
    public void highPassPressed() { filterGenerator.filterChanged(filterGenerator.highPass, 0); }
    public void lowPassPressed() { filterGenerator.filterChanged(filterGenerator.lowPass, 0); }
    public void bandPassPressed() { filterGenerator.filterChanged(filterGenerator.bandPass, 0); }
    public void bandStopPressed() { filterGenerator.filterChanged(filterGenerator.bandStop, 0); }
    public void amplitudePressed() { filterGenerator.timeFilterChanged(filterGenerator.ampFilter, 1, true); }
    public void hlAmpPressed() { 
        filterGenerator.timeFilterChanged(filterGenerator.ampFilter, 1, true); 
        filterGenerator.timeFilterChanged(filterGenerator.ampFilter, -1, false);
    }
}
