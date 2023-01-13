import React, {useEffect, useState} from"react"
import CookTimeBanner from "../components/CookTimeBanner";
import NavigationBar from "../components/NavigationBar/NavigationBar";

export interface HomeProps {
}

function Home(props: HomeProps) {
    return (
      <>
      <NavigationBar />
      <CookTimeBanner />
      </>
    );
}
export default Home;