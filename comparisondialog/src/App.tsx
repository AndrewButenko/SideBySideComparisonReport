import { Dropdown, IDropdownOption, Stack, Toggle } from '@fluentui/react';
import { DetailsList, DetailsListLayoutMode, IColumn } from '@fluentui/react/lib/components/DetailsList';
import React from 'react';
import { OutData } from './OutData';

export interface IAppProps {
  Data: OutData[];
}

export const App: React.FunctionComponent<IAppProps> = props => {
  const primaryField = props.Data.find(a => a.IsPrimary);
  const allFieldTypes = props.Data.map(a => a.AttributeType);
  const uniqueFieldTypes = allFieldTypes
    .filter((a: string, i: number) => allFieldTypes.indexOf(a) === i)
    .sort();
  const allDDOptions: IDropdownOption[] = uniqueFieldTypes.map(o => {
    return {
      key: o,
      text: o,
      selected: true
    };
  });

  const columns: IColumn[] = [
    {
      key: "FieldLabel",
      name: "Entity Field",
      minWidth: 200,
      fieldName: "FieldLabel"
    },
    {
      key: "Record1FieldDisplayValue",
      name: primaryField?.Record1FieldDisplayValue ?? "(No Name)",
      minWidth: 200,
      fieldName: "Record1FieldDisplayValue"

    },
    {
      key: "Record2FieldDisplayValue",
      name: primaryField?.Record2FieldDisplayValue ?? "(No Name)",
      minWidth: 200,
      fieldName: "Record2FieldDisplayValue"
    }
  ];

  const [isShowDiff, setIsShowDiff] = React.useState<boolean>(true);
  const [isShowCustomOnly, setIsShowCustomOnly] = React.useState<boolean>(false);
  const [selectedAttributes, setSelectedAttributes] = React.useState<string[]>(uniqueFieldTypes);

  const fieldsToShow = props.Data
    .filter(d => !isShowDiff || (isShowDiff && !d.IsEqual))
    .filter(d => !isShowCustomOnly || (isShowCustomOnly && d.IsCustom))
    .filter(d => selectedAttributes.some(a => a === d.AttributeType));

  return (
    <>
      <Stack horizontal>
        <Toggle
          onText="Show only differences"
          offText="Show all fields"
          defaultChecked={true}
          checked={isShowDiff}
          onChange={(e: any, checked?: boolean) => {
            setIsShowDiff(!!checked);
          }}
        />
        <Toggle
          onText="Show only custom fields"
          offText="Show all fields"
          defaultChecked={false}
          checked={isShowCustomOnly}
          onChange={(e: any, checked?: boolean) => {
            setIsShowCustomOnly(!!checked);
          }}
        />
      </Stack>
      <Dropdown
          options={allDDOptions}
          multiSelect
          selectedKeys={selectedAttributes}
          onChange={(e: any, selectedOption?: IDropdownOption) => {
            if (!selectedOption) {
              return;
            }

            const currentOptions: string[] = selectedAttributes;

            const updatedOptions: string[] = selectedOption.selected ? [...currentOptions, selectedOption.key as string] :
              currentOptions.filter((key: any) => key !== selectedOption.key);

            setSelectedAttributes(updatedOptions);
          }}
          style={{width: "100%"}}
        />
      <DetailsList
        items={fieldsToShow}
        columns={columns}
        layoutMode={DetailsListLayoutMode.justified}
      />
    </>);
}