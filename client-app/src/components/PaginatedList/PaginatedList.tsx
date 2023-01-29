import React, {useEffect, useState} from "react"
import { Col, Pagination, Row } from "react-bootstrap";
import { useSearchParams } from "react-router-dom";
import { PagedResult } from "src/shared/CookTime";
interface PaginatedListProps<T> {
  element : (item : T) => React.ReactNode
  items: PagedResult<T>
  colClassName? : string
}
export default function PaginatedList<T>(props: PaginatedListProps<T>) {
  const { items, element, colClassName} = props;
  const [searchParams, setSearchParams] = useSearchParams();
  const activePage = Number.parseInt(searchParams.get("page") ?? "1");
  return (
    <>
      <Row>
        {
          items.results.map((item, idx) =>
            <Col className={colClassName} key={idx}>
              {element(item)}
            </Col>)
        }
      </Row>
      { items.pageCount > 1 &&
        <Pagination className="justify-content-center">
          {Array.from({ length: items.pageCount }, (x, i) =>
            <Pagination.Item
              active={(i + 1) === activePage}
              onClick={() => {
              const urlParams = new URLSearchParams(window.location.search);
              urlParams.set("page", (i + 1) === 0 ? "" : encodeURIComponent(i + 1));
              setSearchParams(urlParams);
            }}>{i + 1}</Pagination.Item>
          )}
        </Pagination>
      }
    </>);
}