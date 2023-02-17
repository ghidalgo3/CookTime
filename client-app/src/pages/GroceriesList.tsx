import React, {useEffect, useState} from "react"
import { ShoppingCart } from "src/components/Cart";
import { useTitle } from "src/shared/useTitle";

export default function GroceriesList() {
  useTitle("Groceries List")
  return (
    <ShoppingCart />
  );
}