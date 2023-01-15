using UnityEngine;

public class GameModel : global::Model
{
    public ModelRef<SettingModel> settings = new ModelRef<SettingModel>();

    public void Setup()
    {
        Debug.Log("GameModel => SETUP");
        settings.Model = new SettingModel();
        settings.Model.Setup(this);
    }
}