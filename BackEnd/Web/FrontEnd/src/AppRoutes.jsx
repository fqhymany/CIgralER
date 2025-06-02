import {Counter} from './Pages/Counter';
import {FetchData} from './Pages/FetchData';
import {Home} from './Pages/Home';
import {CreateCaseForm} from './Pages/CreateCaseForm';
import {Cases} from './Pages/Cases';
import {BasicInformation} from './Pages/BasicInformation';
import {Reports} from './Pages/Reports';
import NotFound from './Pages/NotFound';
import Deadline from './Pages/Deadline';
import CaseType from './Pages/CaseType';
import CaseStatus from './Pages/CaseStatus';
import {ForgotPassword} from './Pages/ForgotPassword';
import CasePredefinedSubject from './Pages/CasePredefinedSubject';
import ClientRoleInCase from './Pages/ClientRoleInCase';
import Judge from './Pages/Judge';
import CourtType from './Pages/CourtType';
import CourtSubType from './Pages/CourtSubType';
import Logout from './Pages/Logout';
import Clients from './Pages/Clients';
import {CaseDashboard} from './Pages/CaseDashboard';
import TasksCalendar from './Pages/TasksCalendar';
import {AdminLayout} from './layouts/AdminLayout';
import AdminPanel from './Pages/AdminPanel/index';
import LawyersManagement from './Pages/AdminPanel/LawyersManagement';
import CasesManagement from './Pages/AdminPanel/CasesManagement';
import CalendarManagement from './Pages/AdminPanel/CalendarManagement';
import ReportsManagement from './Pages/AdminPanel/ReportsManagement';
import UserManagement from './Pages/UserManagement';
import CaseView from './Pages/CaseView';
import UserPreferenceForm from './Pages/UserPreferenceForm';
import {FinancialOverview} from './Pages/FinancialOverview';
import AdminRegionPanel from './Pages/AdminRegion/AdminRegionPanel';
import StaffManagement from './Pages/AdminRegion/StaffManagement';
import Chat from './components/Chat/Chat';

const AppRoutes = [
  {
    // index: true,
    path: '/',
    element: <Home />,
    requireAuth: true,
    title: 'خانه',
    breadcrumb: false,
  },
  {
    path: '/counter',
    element: <Counter />,
    requireAuth: true,
    title: 'شمارنده',
  },
  {
    path: '/fetch-data',
    element: <FetchData />,
    requireAuth: true,
    title: 'دریافت اطلاعات',
  },
  {
    path: '/Cases',
    element: <Cases />,
    title: 'پرونده ها',
    requireAuth: false,
    children: [
      {
        path: 'EditCase/:id',
        element: <CreateCaseForm />,
        title: 'ویرایش پرونده',
        requireAuth: false,
      },
      {
        path: 'CaseDashboard/:id',
        element: <CaseDashboard />,
        title: 'مدیریت پرونده ',
        requireAuth: false,
      },
      {
        path: 'CreateCase',
        element: <CreateCaseForm />,
        title: 'ایجاد پرونده جدید',
        requireAuth: false,
      },
    ],
  },
  {
    path: '/AdminRegion',
    element: <AdminRegionPanel />,
    title: 'مدیریت ناحیه',
    requireAuth: true,
  },
  {
    path: '/TasksCalendar',
    element: <TasksCalendar />,
    title: 'تقویم ',
    requireAuth: false,
  },
  {
    path: '/ForgotPassword',
    element: <ForgotPassword />,
    title: 'فراموشی رمز عبور',
    requireAuth: false,
  },
  {
    path: '/Logout',
    element: <Logout />,
    title: 'خروج',
    requireAuth: false,
  },
  {
    path: '/FinancialOverview',
    element: <FinancialOverview />,
    title: 'وضعیت مالی',
    requireAuth: false,
  },
  {
    path: '/BasicInformation',
    element: <BasicInformation />,
    title: 'اطلاعات پایه',
    requireAuth: false,
    children: [
      {
        path: 'CaseType',
        element: <CaseType />,
        title: 'نوع پرونده',
        requireAuth: false,
      },
      {
        path: 'CaseStatus',
        element: <CaseStatus />,
        title: 'وضعیت پرونده',
        requireAuth: false,
      },
      {
        path: 'CasePredefinedSubject',
        element: <CasePredefinedSubject />,
        title: 'موضوع پیش فرض پرونده',
        requireAuth: false,
      },
      {
        path: 'ClientRoleInCase',
        element: <ClientRoleInCase />,
        title: 'نقش موکل در پرونده',
        requireAuth: false,
      },
      {
        path: 'Judge',
        element: <Judge />,
        title: 'قاضی',
        requireAuth: false,
      },
      {
        path: 'CourtType',
        element: <CourtType />,
        title: 'نوع دادگاه',
        requireAuth: false,
      },
      {
        path: 'CourtSubType',
        element: <CourtSubType />,
        requireAuth: false,
        title: 'زیرنوع دادگاه',
      },
      {
        path: 'settings',
        element: <UserPreferenceForm />,
        title: 'تنظیمات',
        requireAuth: false,
      },
    ],
  },
  {
    path: '/Reports',
    element: <Reports />,
    title: 'گزارشات',
  },
  {
    path: '/Deadline',
    element: <Deadline />,
    title: 'مهلت ها',
  },
  {
    path: '/Clients',
    element: <Clients />,
    title: 'موکلین',
    requireAuth: true,
  },
  {
    path: '/UserManagement',
    element: <UserManagement />,
    title: 'مدیریت کاربران',
    requireAuth: true,
  },
  {
    path: '/CaseView',
    element: <CaseView />,
    title: 'نمایش پرونده',
    requireAuth: true,
  },
  {
    path: '/admin',
    element: <AdminLayout />,
    title: 'پنل مدیریت',
    requireAuth: true,
    layoutType: 'admin',
    children: [
      {
        index: true,
        element: <AdminPanel />,
        title: 'داشبورد',
        requireAuth: true,
        layoutType: 'admin',
      },
      {
        path: 'lawyers',
        element: <LawyersManagement />,
        title: 'مدیریت وکلا',
        requireAuth: true,
        layoutType: 'admin',
      },
      {
        path: 'cases',
        element: <CasesManagement />,
        title: 'مدیریت پرونده‌ها',
        requireAuth: true,
        layoutType: 'admin',
      },
      {
        path: 'calendar',
        element: <CalendarManagement />,
        title: 'تقویم جلسات',
        requireAuth: true,
        layoutType: 'admin',
      },
      {
        path: 'reports',
        element: <ReportsManagement />,
        title: 'گزارشات',
        requireAuth: true,
        layoutType: 'admin',
      },
      {
        path: 'staff',
        element: <StaffManagement />,
        title: 'مدیریت کارمندان',
        requireAuth: true,
        layoutType: 'admin',
      },
    ],
  },
  {
    path: '/chat',
    element: <Chat />,
    title: 'گفتگو',
    requireAuth: true,
    layoutType: 'none', 
  },
  {
    path: '*',
    element: <NotFound />,
  },
];

export default AppRoutes;
