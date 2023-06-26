import React, {useEffect, useState} from "react"
import { Helmet } from "react-helmet-async";
import { ShoppingCart } from "src/components/Cart";
import { useTitle } from "src/shared/useTitle";

export const CART_PAGE_PATH = "Cart"

export default function GroceriesList() {
  useTitle("Groceries List")
  return (
    <>
      <Helmet>
        <link rel="canonical" href={`${origin}/${CART_PAGE_PATH}`} />
      </Helmet>
      <ShoppingCart />
    </>
  );
}