// import {
//   CookTimeBanner,
//   NavigationBar,
//   SignUp
// } from "@components";
import { CookTimeBanner, NavigationBar, SignUp } from "@components/index";
import React, {useEffect, useState} from"react"

export interface HomeProps {
}

function Home(props: HomeProps) {
    return (
      <>
      <NavigationBar />
      <CookTimeBanner />
      <SignUp />
      </>
    );
}
export default Home;