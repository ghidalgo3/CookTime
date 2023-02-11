import React from "react"
import imgs from "src/assets";
import { DietDetail } from "src/shared/CookTime";

const imageNameToAsset = new Map<string, string>();
imageNameToAsset.set("whole-grains", imgs.wholeGrains);
imageNameToAsset.set("greens", imgs.greens);
imageNameToAsset.set("cruciferous", imgs.cruciferous);
imageNameToAsset.set("other-vegetables", imgs.otherVegetable);
imageNameToAsset.set("beans", imgs.beans);
imageNameToAsset.set("nuts", imgs.nuts);
imageNameToAsset.set("berries", imgs.berries);
imageNameToAsset.set("whole-grains", imgs.wholeGrains);
imageNameToAsset.set("other-fruit", imgs.otherFruit);
imageNameToAsset.set("flaxseed", imgs.flaxseed);
imageNameToAsset.set("spices", imgs.spices);

export function TodaysTenDisplay({todaysTen} : {todaysTen: DietDetail}) {
  const d = todaysTen.details;
  function TopTenImage({ imageName, present} : {imageName : string, present: boolean}) {
    return <img
      className={`todays-tens-symbols ${present ? "" : "absent"}`}
      alt={imageName}
      title={imageName}
      src={imageNameToAsset.get(imageName)}></img>;
  }
  return (
    <div className="todays-tens-container">
      <TopTenImage imageName="whole-grains" present={d?.hasGrains} />
      <TopTenImage imageName="greens" present={d?.hasGreens}/>
      <TopTenImage imageName="cruciferous" present={d?.hasCruciferousVegetables}/>
      <TopTenImage imageName="other-vegetables" present={d?.hasVegetables}/>
      <TopTenImage imageName="beans" present={d?.hasBeans}/>
      <TopTenImage imageName="nuts" present={d?.hasNutsAndSeeds}/>
      <TopTenImage imageName="berries" present={d?.hasBerries}/>
      <TopTenImage imageName="other-fruit" present={d?.hasFruits}/>
      <TopTenImage imageName="flaxseed" present={d?.hasFlaxseeds}/>
      <TopTenImage imageName="spices" present={d?.hasHerbsAndSpices}/>
    </div>);
}
