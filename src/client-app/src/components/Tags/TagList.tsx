import * as React from "react";

type TagListProps = {
    data: string[],
    onDelete: (index: number) => boolean
}

// Ostensibly this is just the tag list
export class TagList extends React.Component<TagListProps, {}> {
    render() {
        let lis = this.props.data.map((item, i) => {
            return (<span key={i} className="badge">
                <input type="hidden" name={`Skills[${i}]`} value={item}></input>
                {item}
                <i
                    className="bi bi-x margin-left-4"
                    onClick={(e) => this.props.onDelete(i)}></i>
            </span>)
        })
        return (
            <div>
                {lis}
            </div>
        )
    }
}