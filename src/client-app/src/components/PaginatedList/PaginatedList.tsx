import React, { useEffect, useState } from "react"
import { Col, Pagination, Row } from "react-bootstrap";
import { useSearchParams } from "react-router";
import { PagedResult } from "src/shared/CookTime";
import "./PaginatedList.css"
import { RecipeCardInFeedAd } from "../RecipeCardInFeedAd";

interface PaginatedListProps<T> {
  element: (item: T) => React.ReactNode
  items: PagedResult<T>
  colClassName?: string
  inFeedAdIndex?: number
}
export default function PaginatedList<T>(props: PaginatedListProps<T>) {
  const { items, element, colClassName } = props;
  const [searchParams, setSearchParams] = useSearchParams();
  const activePage = Number.parseInt(searchParams.get("page") ?? "1");
  function paramsForPage(i: number) {
    const urlParams = new URLSearchParams(searchParams.toString());
    urlParams.set("page", i === 0 ? "" : encodeURIComponent(i));
    return urlParams;
  }
  function navigateToPage(i: number) {
    setSearchParams(paramsForPage(i));
  }

  let elementInner = (item: T | null, idx: number) => {
    if (idx === props.inFeedAdIndex) {
      return (
        <RecipeCardInFeedAd />
      )
    } else {
      return element(item as T);
    }
  }
  var itemsWithAd: (T | null)[] = items.results;
  if (props.inFeedAdIndex && itemsWithAd.length > 0 && props.inFeedAdIndex < itemsWithAd.length) {
    itemsWithAd = [
      ...itemsWithAd.slice(0, props.inFeedAdIndex),
      null,
      ...itemsWithAd.slice(props.inFeedAdIndex)];
  }
  return (
    <>
      <Row>
        {
          itemsWithAd.map((item, idx) => {
            return (
              <Col
                sm="4"
                className={colClassName}
                key={idx}>
                {elementInner(item, idx)}
              </Col>
            )
          })
          //     if (props.inFeedAdIndex && idx > props.inFeedAdIndex) {
          //   idx--;
          //     }
          // if (props.inFeedAdIndex && idx === props.inFeedAdIndex) {
          //       return (
          // <Col
          //   sm="4"
          //   className={colClassName}
          //   key={idx}>

          //   <RecipeCardInFeedAd />
          // </Col>
          // )
          //     } else {
          //       return (
          // <Col
          //   sm="4"
          //   className={colClassName}
          //   key={idx}>
          //   {element(item)}
          // </Col>
          // )
          // }
          // }
          // )
        }
      </Row>
      {items.pageCount > 1 &&
        <Pagination className="justify-content-center">

          {activePage > 1 &&
            <>
              <a
                style={{ "display": "none" }}
                href={`/?${paramsForPage(activePage - 1).toString()}`}>Crawling link</a>
              <Pagination.Prev onClick={() => navigateToPage(activePage - 1)}>Previous</Pagination.Prev>
            </>
          }

          {Array.from({ length: items.pageCount }, (x, i) =>
            <Pagination.Item
              key={i}
              active={(i + 1) === activePage}
              style={{ color: "black" }}
              onClick={() => {
                navigateToPage(i + 1);
              }}>
              {i + 1}
            </Pagination.Item>
          )}

          {activePage < items.pageCount &&
            <>
              <a
                style={{ "display": "none" }}
                href={`/?${paramsForPage(activePage + 1).toString()}`}>Crawling link</a>
              <Pagination.Next
                onClick={() => { navigateToPage(activePage + 1) }}>
                Next
              </Pagination.Next>
            </>
          }


        </Pagination>
      }
    </>);
}