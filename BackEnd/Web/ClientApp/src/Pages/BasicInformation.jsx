import React from 'react';
import {PiBuildingApartmentLight, PiBuildingOfficeLight, PiFadersLight, PiFileTextLight, PiFileLight, PiFolderLight, PiGearLight, PiToolboxLight, PiUserCheckLight, PiUserListLight} from 'react-icons/pi';
import MainButton from '../components/MainButton/MainButton';
const mainButtons = [
  {
    title: 'نوع پرونده',
    subTitle: '',
    link: '/BasicInformation/CaseType',
    icon: <PiFileTextLight size="55" className="fs-1" color="blue" />,
  },
  {
    title: 'وضعیت پرونده',
    subTitle: '',
    link: '/BasicInformation/CaseStatus',
    icon: <PiFadersLight size="55" className="fs-1" color="blue" />,
  },
  {
    title: 'موضوع پیش فرض پرونده',
    subTitle: '',
    link: '/BasicInformation/CasePredefinedSubject',
    icon: <PiFolderLight size="55" className="fs-1" color="blue" />,
  },
  {
    title: 'نقش موکل در پرونده',
    subTitle: '',
    link: '/BasicInformation/ClientRoleInCase',
    icon: <PiUserListLight size="55" className="fs-1" color="blue" />,
  },
  {
    title: 'قضات',
    subTitle: '',
    link: '/BasicInformation/Judge',
    icon: <PiUserCheckLight size="55" className="fs-1" color="blue" />,
  },
  {
    title: 'نوع دادگاه',
    subTitle: '',
    link: '/BasicInformation/CourtType',
    icon: <PiBuildingApartmentLight size="55" className="fs-1" color="blue" />,
  },
  {
    title: 'زیرنوع دادگاه',
    subTitle: '',
    link: '/BasicInformation/CourtSubType',
    icon: <PiBuildingOfficeLight size="55" className="fs-1" color="blue" />,
  },
  {
    title: 'فاکتور',
    subTitle: '',
    link: '/CreateCaseForm',
    icon: <PiFileLight size="55" className="fs-1" color="blue" />,
  },
  {
    title: 'نوع کار',
    subTitle: '',
    link: '/',
    icon: <PiToolboxLight size="55" className="fs-1" color="blue" />,
  },
  {
    title: 'تنظیمات',
    subTitle: '',
    link: '/BasicInformation/settings',
    icon: <PiGearLight size="55" className="fs-1" color="blue" />,
  },
];

export function BasicInformation() {
  return (
    <>
      {/* <h2 className="main-title-position bg-white p-md-3 p-2 z-2 fs-6">اطلاعات پایه</h2> */}
      <div className="position-relative px-2 py-5 px-md-5 d-flex justify-content-center align-items-center gap-md-5 gap-3 flex-wrap rounded">
        <MainButton buttons={mainButtons} />
      </div>
    </>
  );
}
