import React, {useEffect, useState} from "react"
import { Col, Pagination, Row } from "react-bootstrap";
import { useSearchParams } from "react-router-dom";
import { PagedResult } from "src/shared/CookTime";
import "./PaginatedList.css"

interface PaginatedListProps<T> {
  element : (item : T) => React.ReactNode
  items: PagedResult<T>
  colClassName? : string
}
export default function PaginatedList<T>(props: PaginatedListProps<T>) {
  const { items, element, colClassName} = props;
  const [searchParams, setSearchParams] = useSearchParams();
  const activePage = Number.parseInt(searchParams.get("page") ?? "1");

  function navigateToPage(i: number) {
    const urlParams = new URLSearchParams(window.location.search);
    urlParams.set("page", i === 0 ? "" : encodeURIComponent(i));
    setSearchParams(urlParams);
  }

  return (
    <>
      <Row>
        {
          items.results.map((item, idx) =>
            <Col
              sm="4"
              className={colClassName}
              key={idx}>
              {element(item)}
            </Col>)
        }
      </Row>
      { items.pageCount > 1 &&
        <Pagination className="justify-content-center">
          <Pagination.Prev onClick={() => navigateToPage(activePage - 1)}>Previous</Pagination.Prev>
          {Array.from({ length: items.pageCount }, (x, i) =>
            <Pagination.Item
              key={i}
              active={(i + 1) === activePage}
              style={{color: "black"}}
              onClick={() => {
                navigateToPage(i + 1);
            }}>{i + 1}</Pagination.Item>
          )}
          <Pagination.Next onClick={() => {navigateToPage(activePage + 1)}}>Next</Pagination.Next>
        </Pagination>
      }
    </>);
}