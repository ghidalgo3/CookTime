import React, {useEffect, useState} from "react"
import { Helmet } from "react-helmet-async";
import { useTitle } from "src/shared/useTitle";

export const ABOUT_PAGE_PATH = "About"

export default function About() {
    useTitle("About")
    return (
    <>
      <Helmet>
        <link rel="canonical" href={`${origin}/${ABOUT_PAGE_PATH}`} />
      </Helmet>
      <h1>About</h1>
      <p>
        Welcome to CookTime, your one-stop digital cookbook where flavors, nutrition, and convenience blend seamlessly. 
        At CookTime, we are devoted to turning your kitchen experience into a joyous journey that resonates with your health goals and culinary aspirations. 
        Our platform is a rich database of delectable recipes shared by home cooks and culinary experts from around the world. 
        But CookTime is not just another recipe aggregator. 
        We are a vibrant community of food lovers where you can upload, share, and treasure your own culinary creations, expanding your gastronomic horizon while inspiring others.
      </p>
      <p>
        The core feature of CookTime is its innovative nutritional calculator. 
        No longer do you need to wonder about the nutritional content of your dishes. 
        Just input your recipe and let us do the hard work. 
        We provide comprehensive information about calories, macros, and micronutrients of each dish, helping you align your meals with your dietary preferences or health goals. 
        Whether you're seeking high-protein meals, low-carb delicacies, or heart-healthy options, our platform empowers you with the knowledge you need.
      </p>
      <p>
        But that's not all! CookTime is built to streamline your cooking process. 
        Our smart servings adjuster allows you to easily scale ingredient quantities up or down, ensuring you get the perfect portions every time. 
        No more guesswork or complicated calculations! And when it comes to shopping, CookTime has you covered with its smart groceries list feature. 
        Select your recipes for the week, and the platform will automatically aggregate the ingredients from multiple recipes into a single, easy-to-use list. 
        Now, making your shopping list is as easy as pie!
      </p>
      <p>
        Our user-friendly interface is enhanced by cutting-edge AI features, making CookTime a pioneer in the culinary tech field. 
        Our magically easy recipe import feature recognizes ingredients and instructions from any image, allowing you to instantly import and save recipes from your favorite books or handwritten notes. 
        Say goodbye to tedious manual entries and embrace the convenience of AI-powered cooking. 
        Join us at CookTime, where we are cooking up the future, one recipe at a time!
      </p>    
    </>);
  }